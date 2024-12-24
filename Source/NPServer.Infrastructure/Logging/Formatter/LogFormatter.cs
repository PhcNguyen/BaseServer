using NPServer.Infrastructure.Logging.Interfaces;
using System;
using System.Text;

namespace NPServer.Infrastructure.Logging.Formatter;

/// <summary>
/// Lớp định dạng cho thông điệp nhật ký.
/// </summary>
public sealed class LogFormatter : ILogFormatter
{
    /// <summary>
    /// Khởi tạo một đối tượng <see cref="LogFormatter"/>.
    /// </summary>
    public LogFormatter()
    {
    }

    /// <summary>
    /// Áp dụng định dạng cho thông điệp log.
    /// </summary>
    /// <param name="logMessage">Đối tượng thông điệp log cần định dạng.</param>
    /// <returns>Chuỗi định dạng của thông điệp log.</returns>
    public string ApplyFormat(LogMessage logMessage)
    {
        ArgumentNullException.ThrowIfNull(logMessage);

        StringBuilder logBuilder = new();

        logBuilder.AppendFormat("{0:HH:mm:ss.fff}", logMessage.DateTime);
        logBuilder.Append(" - ");
        logBuilder.Append(logMessage.Level);
        logBuilder.Append(" - ");

        if (!string.IsNullOrEmpty(logMessage.CallingClass) && !string.IsNullOrEmpty(logMessage.CallingMethod))
        {
            logBuilder.AppendFormat("[{0} -> {1}()]: ", logMessage.CallingClass, logMessage.CallingMethod);
        }

        logBuilder.Append(logMessage.Text);

        return logBuilder.ToString();
    }

    /// <summary>
    /// Định dạng thông điệp ngoại lệ thành chuỗi.
    /// </summary>
    /// <param name="exception">Ngoại lệ cần định dạng.</param>
    /// <returns>Chuỗi định dạng thông điệp ngoại lệ.</returns>
    public static string FormatExceptionMessage(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        StringBuilder exceptionBuilder = new();
        exceptionBuilder.AppendLine("Log exception -> ");
        exceptionBuilder.AppendLine($"Message: {exception.Message}");
        exceptionBuilder.AppendLine($"StackTrace: {exception.StackTrace}");

        if (exception.InnerException != null)
        {
            exceptionBuilder.AppendLine($"InnerException: {exception.InnerException.Message}");
            exceptionBuilder.AppendLine($"InnerException StackTrace: {exception.InnerException.StackTrace}");
        }

        return exceptionBuilder.ToString();
    }
}