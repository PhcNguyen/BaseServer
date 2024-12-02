﻿using NServer.Infrastructure.Logging.Formatter;
using NServer.Infrastructure.Logging.Interfaces;
using System;

namespace NServer.Infrastructure.Logging.Filter;

/// <summary>
/// Bộ lọc các thông điệp nhật ký trước khi xử lý bằng handler cụ thể.
/// </summary>
internal class FilteredHandler : INLogHandler
{
    /// <summary>
    /// Bộ lọc để xác định xem thông điệp nhật ký có nên được xử lý hay không.
    /// </summary>
    public Predicate<LogMessage>? Filter { get; set; }

    /// <summary>
    /// Handler để xử lý thông điệp nhật ký sau khi đã được lọc.
    /// </summary>
    public INLogHandler? Handler { get; set; }

    /// <summary>
    /// Công khai một thông điệp nhật ký nếu thông điệp thỏa mãn bộ lọc.
    /// </summary>
    /// <param name="logMessage">Thông điệp nhật ký cần công khai.</param>
    public void Publish(LogMessage logMessage)
    {
        if (Filter!(logMessage))
            Handler!.Publish(logMessage);
    }
}