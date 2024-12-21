using NPServer.Infrastructure.Logging.Formatter;

namespace NPServer.Infrastructure.Logging.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho định dạng thông điệp log.
/// </summary>
public interface ILogFormatter
{
    /// <summary>
    /// Áp dụng định dạng cho thông điệp log.
    /// </summary>
    /// <param name="logMessage">Thông điệp log cần được định dạng.</param>
    /// <returns>Chuỗi văn bản đã định dạng.</returns>
    string ApplyFormat(LogMessage logMessage);
}
