using NPServer.Infrastructure.Logging.Formatter;
using NPServer.Infrastructure.Logging.Targets;
using System;

namespace NPServer.Infrastructure.Logging;

/// <summary>
/// Lớp chính cho hệ thống ghi nhật ký, cung cấp các phương thức để ghi nhật ký thông qua các handler khác nhau.
/// </summary>
public sealed class NPLog : NPLogBase
{
    private static readonly Lazy<NPLog> _instance = new(() => new NPLog());

    /// <summary>
    /// Constructor riêng tư cho Singleton.
    /// </summary>
    private NPLog()
    { }

    /// <summary>
    /// Trả về instance duy nhất của <see cref="NPLog"/>.
    /// </summary>
    public static NPLog Instance => _instance.Value;

    /// <summary>
    /// Khởi tạo mặc định các handler.
    /// </summary>
    public void DefaultInitialization()
    {
        LoggerHandlerManager
            .AddHandler(new ConsoleTarget())
            .AddHandler(new FileTarget());
        Log(Level.INFO, "Default initialization");
    }

    /// <summary>
    /// Thay đổi mức độ log động.
    /// </summary>
    public void SetLogLevel(Level level)
    {
        base.CurrentLogLevel = level;
        Log(Level.INFO, $"Log level changed to: {level}");
    }

    /// <summary>
    /// Kiểm tra xem mức độ nhật ký có nên được ghi lại hay không.
    /// </summary>
    /// <param name="level">Mức độ nhật ký cần kiểm tra.</param>
    /// <returns>True nếu mức độ nhật ký lớn hơn hoặc bằng mức độ mặc định, ngược lại False.</returns>
    public bool ShouldLog(Level level) => level >= base.CurrentLogLevel;

    /// <summary>
    /// Ghi một thông điệp với mức độ mặc định.
    /// </summary>
    public void Log(string message) => Log(base.CurrentLogLevel, message);

    /// <summary>
    /// Ghi một thông điệp với mức độ chỉ định và thông tin chi tiết từ stack trace.
    /// </summary>
    /// <param name="level">Mức độ nhật ký.</param>
    /// <param name="message">Thông điệp nhật ký.</param>
    public void Log(Level level, string message) =>
        LogInternal(level, message, null, null);

    /// <summary>
    /// Ghi một thông điệp với mức độ và lớp chỉ định.
    /// </summary>
    /// <typeparam name="TClass">Lớp ghi nhật ký.</typeparam>
    /// <param name="level">Mức độ nhật ký.</param>
    /// <param name="message">Thông điệp nhật ký.</param>
    public void Log<TClass>(Level level, string message) where TClass : class =>
        LogInternal(level, message, typeof(TClass).Name, GetCallerMethodName());

    /// <summary>
    /// Ghi một thông điệp với mức độ và lớp chỉ định, cùng với delegate.
    /// </summary>
    public void Log<TClass, TFunc>(Level level, string message)
        where TClass : class
        where TFunc : Delegate =>
        LogInternal(level, message, typeof(TClass).Name, typeof(TFunc).Name);

    /// <summary>
    /// Ghi một thông điệp với mức độ INFO.
    /// </summary>
    public void Info(string message) => Log(Level.INFO, message);

    /// <summary>
    /// Ghi một thông điệp với mức độ INFO.
    /// </summary>
    public void Info<TClass>(string message) where TClass : class =>
        Log<TClass>(Level.INFO, message);

    /// <summary>
    /// Ghi một thông điệp với mức độ DEBUG.
    /// </summary>
    /// <param name="message">Thông điệp cần ghi.</param>
    public void Debug(string message) => Log(Level.DEBUG, message);

    /// <summary>
    /// Ghi một thông điệp với mức độ DEBUG.
    /// </summary>
    /// <param name="message">Thông điệp cần ghi.</param>
    public void Debug<TClass>(string message) where TClass : class =>
        Log<TClass>(Level.DEBUG, message);

    /// <summary>
    /// Ghi một thông điệp với mức độ TRACE.
    /// </summary>
    /// <param name="message">Thông điệp cần ghi.</param>
    public void Trace(string message) => Log(Level.TRACE, message);

    /// <summary>
    /// Ghi một thông điệp với mức độ AUDIT.
    /// </summary>
    /// <param name="message">Thông điệp cần ghi.</param>
    public void Audit(string message) => Log(Level.AUDIT, message);

    /// <summary>
    /// Ghi một thông điệp với mức độ SECURITY.
    /// </summary>
    /// <param name="message">Thông điệp cần ghi.</param>
    public void Security(string message) => Log(Level.SECURITY, message);

    /// <summary>
    /// Ghi một thông điệp cảnh báo.
    /// </summary>
    /// <param name="message">Thông điệp cần ghi.</param>
    public void Warning(string message) => Log(Level.WARNING, message);

    /// <summary>
    /// Ghi một thông điệp cảnh báo.
    /// </summary>
    /// <param name="message">Thông điệp cần ghi.</param>
    public void Warning<TClass>(string message) where TClass : class =>
        Log<TClass>(Level.WARNING, message);

    /// <summary>
    /// Ghi một thông điệp lỗi kèm theo ngoại lệ.
    /// </summary>
    /// <param name="exception">Đối tượng ngoại lệ liên quan.</param>
    public void Error(Exception exception) =>
        Log(Level.ERROR, LogFormatter.FormatExceptionMessage(exception));

    /// <summary>
    /// Ghi một thông điệp lỗi kèm theo thông điệp và ngoại lệ.
    /// </summary>
    /// <param name="message">Thông điệp lỗi.</param>
    /// <param name="exception">Đối tượng ngoại lệ liên quan.</param>
    public void Error(string message, Exception exception) =>
        Log(Level.ERROR, $"{message}: {LogFormatter.FormatExceptionMessage(exception)}");

    /// <summary>
    /// Ghi một thông điệp lỗi kèm theo thông điệp và ngoại lệ.
    /// </summary>
    /// <param name="message">Thông điệp lỗi.</param>
    public void Error<TClass>(string message) where TClass : class =>
        Log<TClass>(Level.ERROR, message);

    /// <summary>
    /// Ghi một thông điệp lỗi kèm theo thông điệp và ngoại lệ.
    /// </summary>
    /// <param name="exception">Đối tượng ngoại lệ liên quan.</param>
    public void Error<TClass>(Exception exception) where TClass : class =>
        Log<TClass>(Level.ERROR, LogFormatter.FormatExceptionMessage(exception));

    /// <summary>
    /// Ghi một thông điệp lỗi kèm theo thông điệp và ngoại lệ.
    /// </summary>
    /// <param name="message">Thông điệp lỗi.</param>
    /// <param name="exception">Đối tượng ngoại lệ liên quan.</param>
    public void Error<TClass>(string message, Exception exception) where TClass : class =>
        Log<TClass>(Level.ERROR, $"{message}: {LogFormatter.FormatExceptionMessage(exception)}");
}