﻿using NPServer.Infrastructure.Logging.Formatter;

namespace NPServer.Infrastructure.Logging.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho mục tiêu xử lý nhật ký.
/// </summary>
public interface INPLogTarget
{
    /// <summary>
    /// Gửi một thông điệp log đến mục tiêu xử lý.
    /// </summary>
    /// <param name="logMessage">Đối tượng thông điệp log.</param>
    void Publish(LogMessage logMessage);

    /// <summary>
    /// Giải phóng tài nguyên khi không còn sử dụng.
    /// </summary>
    void Dispose();
}
