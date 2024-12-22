using NPServer.Infrastructure.Logging.Formatter;
using NPServer.Infrastructure.Logging.Interfaces;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System;


namespace NPServer.Infrastructure.Logging.Targets;

/// <summary>
/// Lớp WinFormTagers cung cấp khả năng xuất thông điệp nhật ký ra WinForm.
/// </summary>
public class WinFormTagers : INPLogTarget, IDisposable
{
    private readonly Task _workerTask;
    private readonly INLogPrintTagers _textBox;
    private readonly ILogFormatter _loggerFormatter;
    private readonly ConcurrentQueue<LogMessage> _logQueue = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    /// <summary>
    /// Khởi tạo đối tượng ConsoleTarget với định dạng log cụ thể.
    /// </summary>
    /// <param name="textBox">Đối tượng thực hiện thị log.</param>
    /// <param name="loggerFormatter">Đối tượng thực hiện định dạng log.</param>
    public WinFormTagers(INLogPrintTagers textBox, ILogFormatter loggerFormatter)
    {
        _textBox = textBox ?? throw new ArgumentNullException(nameof(textBox));
        _loggerFormatter = loggerFormatter ?? throw new ArgumentNullException(nameof(loggerFormatter));

        _workerTask = Task.Run(() => ProcessLogQueueAsync(_cancellationTokenSource.Token));
    }

    /// <summary>
    /// Khởi tạo đối tượng ConsoleTarget với định dạng mặc định.
    /// </summary>
    public WinFormTagers(INLogPrintTagers textBox) : this(textBox, new LogFormatter())
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
                _textBox.WriteLine(_loggerFormatter.ApplyFormat(logMessage));
            }
            else
            {
                await Task.Delay(10, cancellationToken); // Giữ cho luồng chạy nhẹ nhàng nếu không có log nào
            }
        }
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
