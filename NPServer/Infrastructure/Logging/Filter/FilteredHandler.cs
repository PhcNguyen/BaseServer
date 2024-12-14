using NPServer.Infrastructure.Logging.Formatter;
using NPServer.Infrastructure.Logging.Interfaces;
using System;

namespace NPServer.Infrastructure.Logging.Filter;

/// <summary>
/// Bộ lọc các thông điệp nhật ký trước khi xử lý bằng handler cụ thể.
/// </summary>
internal class FilteredHandler : INPLogTarget
{
    /// <summary>
    /// Bộ lọc để xác định xem thông điệp nhật ký có nên được xử lý hay không.
    /// </summary>
    public Predicate<LogMessage>? Filter { get; set; }

    /// <summary>
    /// Handler để xử lý thông điệp nhật ký sau khi đã được lọc.
    /// </summary>
    public INPLogTarget? Handler { get; set; }

    /// <summary>
    /// Công khai một thông điệp nhật ký nếu thông điệp thỏa mãn bộ lọc.
    /// </summary>
    /// <param name="logMessage">Thông điệp nhật ký cần công khai.</param>
    public void Publish(LogMessage logMessage)
    {
        if (Filter!(logMessage))
            Handler!.Publish(logMessage);
    }

    public void Dispose()
    { }
}