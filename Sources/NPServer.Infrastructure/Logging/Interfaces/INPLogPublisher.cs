using NPServer.Infrastructure.Logging.Formatter;
using System;

namespace NPServer.Infrastructure.Logging.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho nhà xuất bản nhật ký.
/// </summary>
public interface INPLogPublisher
{
    /// <summary>
    /// Thêm một đối tượng xử lý nhật ký.
    /// </summary>
    /// <param name="loggerHandler">Đối tượng xử lý nhật ký.</param>
    /// <returns>Đối tượng <see cref="INPLogPublisher"/> hiện tại.</returns>
    INPLogPublisher AddHandler(INPLogTarget loggerHandler);

    /// <summary>
    /// Thêm một đối tượng xử lý nhật ký với bộ lọc.
    /// </summary>
    /// <param name="loggerHandler">Đối tượng xử lý nhật ký.</param>
    /// <param name="filter">Predicate dùng để lọc các thông điệp log.</param>
    /// <returns>Đối tượng <see cref="INPLogPublisher"/> hiện tại.</returns>
    INPLogPublisher AddHandler(INPLogTarget loggerHandler, Predicate<LogMessage> filter);

    /// <summary>
    /// Xóa một đối tượng xử lý nhật ký.
    /// </summary>
    /// <param name="loggerHandler">Đối tượng xử lý nhật ký cần xóa.</param>
    /// <returns><c>true</c> nếu đối tượng xử lý đã được xóa thành công, <c>false</c> nếu không.</returns>
    bool RemoveHandler(INPLogTarget loggerHandler);
}