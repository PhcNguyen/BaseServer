using NPServer.Infrastructure.Logging.Formatter;
using NPServer.Infrastructure.Logging.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Infrastructure.Logging.Targets;

/// <summary>
/// Lớp ConsoleTarget cung cấp khả năng xuất thông điệp nhật ký ra console với màu sắc tương ứng với mức độ log.
/// </summary>
public sealed class ConsoleTarget : INPLogTarget, IDisposable
{
    private readonly ILogFormatter _loggerFormatter;
    private readonly ConcurrentQueue<LogMessage> _logQueue = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Task _workerTask;

    /// <summary>
    /// Khởi tạo đối tượng ConsoleTarget với định dạng log cụ thể.
    /// </summary>
    /// <param name="loggerFormatter">Đối tượng thực hiện định dạng log.</param>
    public ConsoleTarget(ILogFormatter loggerFormatter)
    {
        _loggerFormatter = loggerFormatter ?? throw new ArgumentNullException(nameof(loggerFormatter));

        _workerTask = Task.Run(() => ProcessLogQueueAsync(_cancellationTokenSource.Token));
    }

    /// <summary>
    /// Khởi tạo đối tượng ConsoleTarget với định dạng mặc định.
    /// </summary>
    public ConsoleTarget() : this(new LogFormatter())
    {
    }

    /// <summary>
    /// Xuất thông điệp log ra console.
    /// </summary>
    /// <param name="logMessage">Thông điệp log cần xuất.</param>
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

    /// <summary>
    /// Đặt màu sắc cho mức độ log.
    /// </summary>
    /// <param name="level">Mức độ log cần đặt màu sắc.</param>
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

    /// <summary>
    /// Giải phóng tài nguyên và hủy bỏ luồng xử lý log.
    /// </summary>
    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _workerTask.Wait();

        _cancellationTokenSource.Dispose();

        GC.SuppressFinalize(this);
    }
}