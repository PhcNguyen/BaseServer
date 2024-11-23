﻿using NServer.Infrastructure.Configuration;
using NServer.Infrastructure.Logging.Enums;
using System.Text;
using System.Collections.Concurrent;

namespace NServer.Infrastructure.Logging.Helpers
{
    internal class FileLogging : IDisposable
    {
        private readonly BlockingCollection<(string Message, LogLevel Level)> _logQueue = new();
        private readonly List<string> _currentBatch = new(LoggingConfigs.BatchSize);
        private CancellationTokenSource _cancellationTokenSource = new();
        private Timer? _flushTimer;
        private Task? _logTask;
        private bool _isDisposed = false;

        public FileLogging()
        {
            Start();
        }

        // Lấy đường dẫn file dựa trên LogLevel
        private static string GetFilePath(LogLevel level)
        {
            return Config.LogLevelFileMappings.GetValueOrDefault(level, Config.DefaultLogFilePath);
        }

        // Phương thức xử lý ghi log bất đồng bộ
        private async Task ProcessLogQueue()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested || !_logQueue.IsCompleted)
            {
                try
                {
                    // Lấy mục log từ queue
                    if (_logQueue.TryTake(out var logItem, Timeout.Infinite, _cancellationTokenSource.Token))
                    {
                        _currentBatch.Add(logItem.Message);

                        // Nếu đạt đến kích thước batch thì ghi log vào file
                        if (_currentBatch.Count >= LoggingConfigs.BatchSize)
                        {
                            await FlushAsync(logItem.Level);  // Sử dụng LogLevel từ item trong queue
                        }
                    }
                }
                catch (OperationCanceledException) { /* Thoát khi bị hủy */ }
                catch (Exception ex) { NLog.Error($"Error processing log queue: {ex.Message}"); }
            }
        }

        private async Task FlushAsync(LogLevel level)
        {
            if (_currentBatch.Count == 0) return;

            // Lấy file path từ LogLevel
            string filePath = GetFilePath(level);

            // Sử dụng StreamWriter để ghi log vào file
            using var writer = new StreamWriter(
                new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite),
                Encoding.UTF8, bufferSize: 8192, leaveOpen: false  // Tăng kích thước buffer
            );

            foreach (var log in _currentBatch)
            {
                await writer.WriteLineAsync(log);  // Ghi bất đồng bộ
            }

            _currentBatch.Clear();  // Dọn dẹp sau khi ghi
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _logTask = Task.Run(ProcessLogQueue);

            // Bắt đầu quá trình ghi log bất đồng bộ theo định kỳ
            _flushTimer = new Timer(
                async _ => await FlushAsync(LogLevel.INFO),
                null,
                LoggingConfigs.InitialFlushDelay,
                LoggingConfigs.FlushInterval
            );
        }

        // Gửi log vào file bất đồng bộ
        public void QueueLog(string message, LogLevel level)
        {
            _logQueue.Add((message, level));
        }

        public void Shutdown()
        {
            _cancellationTokenSource.Cancel();
            _flushTimer?.Dispose();

            _logQueue.CompleteAdding();
            _logTask?.GetAwaiter().GetResult();

            // Ghi log còn lại một cách đồng bộ, với LogLevel mặc định
            FlushAsync(LogLevel.INFO).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                Shutdown();
                _cancellationTokenSource.Dispose();
                _isDisposed = true;
            }
        }
    }
}