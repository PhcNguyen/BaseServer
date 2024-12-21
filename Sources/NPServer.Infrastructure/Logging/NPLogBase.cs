using NPServer.Infrastructure.Logging.Formatter;
using NPServer.Infrastructure.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NPServer.Infrastructure.Logging;

/// <summary>
/// Lớp cơ sở cho hệ thống ghi nhật ký, cung cấp các phương thức cơ bản để ghi nhật ký.
/// </summary>
public abstract class NPLogBase
{
    /// <summary>
    /// Defines the log level for logging messages.
    /// </summary>
    public enum Level
    {
        /// <summary>
        /// Không ghi nhật ký.
        /// </summary>
        NONE,

        /// <summary>
        /// Cấp độ ghi nhật ký chi tiết nhất.
        /// </summary>
        TRACE,

        /// <summary>
        /// Ghi lại thông tin chi tiết dùng cho gỡ lỗi.
        /// </summary>
        DEBUG,

        /// <summary>
        /// Ghi nhật ký thông tin chung.
        /// </summary>
        INFO,

        /// <summary>
        /// Ghi cảnh báo về vấn đề cần chú ý.
        /// </summary>
        WARNING,

        /// <summary>
        /// Ghi thông tin lỗi.
        /// </summary>
        ERROR,

        /// <summary>
        /// Ghi thông tin các lỗi nghiêm trọng.
        /// </summary>
        CRITICAL,

        /// <summary>
        /// Ghi lại các sự kiện quan trọng, như thay đổi cấu hình.
        /// </summary>
        AUDIT,

        /// <summary>
        /// Ghi lại các sự kiện bảo mật, như đăng nhập thất bại.
        /// </summary>
        SECURITY
    }

    private readonly NPLogPublisher _logPublisher = new();
    private bool _isTurned = true;

    /// <summary>
    /// Mức độ ghi nhật ký mặc định.
    /// </summary>
    protected Level CurrentLogLevel { get; set; } = Level.INFO;

    /// <summary>
    /// Danh sách các thông điệp nhật ký.
    /// </summary>
    protected IEnumerable<LogMessage> Messages => _logPublisher.Messages;

    /// <summary>
    /// Lấy đối tượng quản lý ghi nhật ký.
    /// </summary>
    protected INPLogPublisher LoggerHandlerManager => _logPublisher;

    /// <summary>
    /// Thiết lập hoặc lấy trạng thái lưu trữ thông điệp nhật ký.
    /// </summary>
    protected bool StoreLogMessages
    {
        get => _logPublisher.StoreLogMessages;
        set => _logPublisher.StoreLogMessages = value;
    }

    /// <summary>
    /// Bật ghi nhật ký.
    /// </summary>
    public void On() => _isTurned = true;

    /// <summary>
    /// Tắt ghi nhật ký.
    /// </summary>
    public void Off() => _isTurned = false;

    /// <summary>
    /// Ghi một thông điệp với mức độ chỉ định.
    /// </summary>
    protected void LogInternal(Level level, string message, string? callingClass, string? callingMethod)
    {
        if (!_isTurned) return;
        var logMessage = new LogMessage(level, message, DateTime.Now, callingClass, callingMethod);

        _logPublisher.Publish(logMessage);
    }

    /// <summary>
    /// Lấy tên của phương thức gọi.
    /// </summary>
    protected static string GetCallerMethodName()
    {
        var method = new StackTrace().GetFrame(3)?.GetMethod();
        return method?.Name ?? "UnknownMethod";
    }
}