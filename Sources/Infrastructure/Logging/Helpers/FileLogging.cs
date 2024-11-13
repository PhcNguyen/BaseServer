using NETServer.Infrastructure.Configuration;
using System.Collections.Concurrent;
using System.Text;

namespace NETServer.Infrastructure.Logging.Helpers
{
    internal static class FileLogging
    {
        private static readonly BlockingCollection<(string Message, LogLevel Level)> _logQueue = [];
        private static readonly List<string> _currentBatch = new(LoggingConfigs.BatchSize); 
        private static CancellationTokenSource _cancellationTokenSource = new();
        private static System.Threading.Timer? _flushTimer;
        private static Task? _logTask;

        // Lấy đường dẫn file dựa trên LogLevel
        private static string GetFilePath(LogLevel level)
        {
            return FileManager.LogLevelFileMappings.GetValueOrDefault(level, FileManager.DefaultLogFilePath);
        }

        private static async Task ProcessLogQueue()
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
                catch (Exception) { /* Xử lý ngoại lệ nếu cần */ }
            }
        }

        private static async Task FlushAsync(LogLevel level)
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

        static FileLogging() => Start();

        public static void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _logTask = Task.Run(ProcessLogQueue);

            _flushTimer = new System.Threading.Timer(
                async _ => await FlushAsync(LogLevel.Info),  // Ghi log bất đồng bộ theo LogLevel
                null,
                LoggingConfigs.InitialFlushDelay,
                LoggingConfigs.FlushInterval
            );
        }

        // Gửi log vào queue, kèm theo cả thông tin LogLevel
        public static void QueueLog(string message, LogLevel level)
        {
            _logQueue.Add((message, level));
        }

        public static void Shutdown()
        {
            _cancellationTokenSource.Cancel();
            _flushTimer?.Dispose();

            _logQueue.CompleteAdding();
            _logTask?.GetAwaiter().GetResult();

            // Ghi log còn lại một cách đồng bộ, với LogLevel mặc định
            FlushAsync(LogLevel.Info).GetAwaiter().GetResult();
        }
    }
}