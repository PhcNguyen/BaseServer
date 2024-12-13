using NPServer.Infrastructure.Logging.Formatter;
using NPServer.Infrastructure.Logging.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Infrastructure.Logging.Targets
{
    public class ConsoleTarget : INPLogTarget, IDisposable
    {
        private readonly ILogFormatter _loggerFormatter;
        private readonly ConcurrentQueue<LogMessage> _logQueue = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly Task _workerTask;

        public ConsoleTarget(ILogFormatter loggerFormatter)
        {
            _loggerFormatter = loggerFormatter ?? throw new ArgumentNullException(nameof(loggerFormatter));

            _workerTask = Task.Run(() => ProcessLogQueueAsync(_cancellationTokenSource.Token));
        }

        public ConsoleTarget() : this(new LogFormatter())
        {
        }

        public void Publish(LogMessage logMessage)
        {
            ArgumentNullException.ThrowIfNull(logMessage);
            _logQueue.Enqueue(logMessage);
        }

        private async Task ProcessLogQueueAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_logQueue.TryDequeue(out var logMessage))
                {
                    try
                    {
                        // Lấy màu tương ứng với mức log
                        SetForegroundColor(logMessage.Level);
                        Console.WriteLine(_loggerFormatter.ApplyFormat(logMessage));
                    }
                    finally
                    {
                        Console.ResetColor(); // Đảm bảo reset màu sau khi in
                    }
                }
                else
                {
                    await Task.Delay(10, cancellationToken); // Giữ cho luồng chạy nhẹ nhàng nếu không có log nào
                }
            }
        }

        private static void SetForegroundColor(NPLogBase.Level level)
        {
            var color = level switch
            {
                NPLogBase.Level.NONE => ConsoleColor.Cyan,
                NPLogBase.Level.INFO => ConsoleColor.White,
                NPLogBase.Level.DEBUG => ConsoleColor.Green,
                NPLogBase.Level.WARNING => ConsoleColor.Yellow,
                NPLogBase.Level.ERROR => ConsoleColor.Magenta,
                NPLogBase.Level.CRITICAL => ConsoleColor.Red,
                NPLogBase.Level.AUDIT => ConsoleColor.Blue,
                NPLogBase.Level.SECURITY => ConsoleColor.DarkRed,
                NPLogBase.Level.TRACE => ConsoleColor.Gray,
                _ => ConsoleColor.White, // Mặc định
            };
            Console.ForegroundColor = color;
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