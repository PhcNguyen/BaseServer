using NPServer.Infrastructure.Logging.Formatter;
using NPServer.Infrastructure.Logging.Interfaces;
using NPServer.Infrastructure.Settings;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Infrastructure.Logging.Targets
{
    public class FileTarget : INPLogTarget, IDisposable
    {
        private readonly string _directory;
        private readonly ILogFormatter _loggerFormatter;
        private readonly ConcurrentQueue<LogMessage> _logQueue;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _workerTask;

        public FileTarget(ILogFormatter loggerFormatter, string directory)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            _loggerFormatter = loggerFormatter ?? throw new ArgumentNullException(nameof(loggerFormatter));
            _logQueue = new ConcurrentQueue<LogMessage>();
            _cancellationTokenSource = new CancellationTokenSource();

            // Đảm bảo thư mục tồn tại
            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }

            // Worker task để xử lý ghi log bất đồng bộ
            _workerTask = Task.Run(ProcessLogQueueAsync, _cancellationTokenSource.Token);
        }

        public FileTarget() : this(new LogFormatter(), LoggingCongfig.LogDirectory)
        {
        }

        public FileTarget(string directory) : this(new LogFormatter(), directory)
        {
        }

        public void Publish(LogMessage logMessage)
        {
            ArgumentNullException.ThrowIfNull(logMessage);
            _logQueue.Enqueue(logMessage); // Đưa log vào hàng đợi
        }

        private async Task ProcessLogQueueAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (_logQueue.TryDequeue(out var logMessage))
                {
                    try
                    {
                        await WriteLogAsync(logMessage);
                    }
                    catch (Exception ex)
                    {
                        // Xử lý ngoại lệ khi ghi log thất bại
                        Console.Error.WriteLine($"Failed to write log: {ex.Message}");
                    }
                }
                else
                {
                    await Task.Delay(100); // Chờ một chút nếu hàng đợi rỗng
                }
            }
        }

        private async Task WriteLogAsync(LogMessage logMessage)
        {
            string filePath = Path.Combine(_directory, CreateFileName(logMessage.Level));
            string logEntry = _loggerFormatter.ApplyFormat(logMessage);

            // Ghi log vào file
            byte[] logBytes = Encoding.UTF8.GetBytes(logEntry + Environment.NewLine);
            using FileStream stream = new(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            await stream.WriteAsync(logBytes);
        }

        private static string CreateFileName(NPLogBase.Level level)
        {
            // Định dạng file: yyyy-MM-dd-INFO.log
            return $"{level}.log";
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _workerTask.Wait();

            _cancellationTokenSource.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}