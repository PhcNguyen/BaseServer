using NPServer.Infrastructure.Logging.Formatter;
using NPServer.Infrastructure.Logging.Interfaces;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Infrastructure.Logging.Targets;

/// <summary>
/// Lớp FileTarget cung cấp khả năng ghi thông điệp log vào file với hỗ trợ đa luồng và định dạng log tùy chỉnh.
/// </summary>
public sealed class FileTarget : INPLogTarget, IDisposable
{
    private readonly string _directory;
    private readonly ILogFormatter _loggerFormatter;
    private readonly ConcurrentQueue<LogMessage> _logQueue;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _workerTask;

    /// <summary>
    /// Khởi tạo đối tượng FileTarget với định dạng log và thư mục lưu trữ.
    /// </summary>
    /// <param name="loggerFormatter">Đối tượng thực hiện định dạng log.</param>
    /// <param name="directory">Thư mục nơi các file log được lưu trữ.</param>
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

    /// <summary>
    /// Khởi tạo đối tượng FileTarget với định dạng mặc định và thư mục mặc định.
    /// </summary>
    public FileTarget() : this(new LogFormatter(), NPLogCongfig.LogDirectory)
    {
    }

    /// <summary>
    /// Khởi tạo đối tượng FileTarget với thư mục lưu trữ.
    /// </summary>
    /// <param name="directory">Thư mục nơi các file log được lưu trữ.</param>
    public FileTarget(string directory) : this(new LogFormatter(), directory)
    {
    }

    /// <summary>
    /// Xuất thông điệp log vào hàng đợi để ghi vào file.
    /// </summary>
    /// <param name="logMessage">Thông điệp log cần xuất.</param>
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

    /// <summary>
    /// Ghi thông điệp log vào file theo định dạng và file cụ thể.
    /// </summary>
    /// <param name="logMessage">Thông điệp log cần ghi.</param>
    private async Task WriteLogAsync(LogMessage logMessage)
    {
        string filePath = Path.Combine(_directory, CreateFileName(logMessage.Level));
        string logEntry = _loggerFormatter.ApplyFormat(logMessage);

        // Ghi log vào file
        byte[] logBytes = Encoding.UTF8.GetBytes(logEntry + Environment.NewLine);
        using FileStream stream = new(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
        await stream.WriteAsync(logBytes);
    }

    /// <summary>
    /// Tạo tên file log theo mức độ log.
    /// </summary>
    /// <param name="level">Mức độ log cần tạo tên file.</param>
    private static string CreateFileName(NPLogBase.Level level)
    {
        // Định dạng file: yyyy-MM-dd-INFO.log
        return $"{level}.log";
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