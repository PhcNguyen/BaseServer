namespace NETServer.Logging;

/// <summary>
/// Defines the log level for logging messages.
/// </summary>
public enum NLogLevel
{
    /// <summary>
    /// Cấp độ log nghiêm trọng nhất, dùng để ghi lại các lỗi có thể làm hệ thống sập hoặc ngừng hoạt động.
    /// </summary>
    Critical,

    /// <summary>
    /// Cấp độ log dùng để ghi lại các lỗi, thường cần phải được xử lý ngay lập tức.
    /// </summary>
    Error,

    /// <summary>
    /// Cấp độ log cảnh báo, dùng để ghi lại các vấn đề không nghiêm trọng nhưng có thể ảnh hưởng đến hoạt động của ứng dụng.
    /// </summary>
    Warning,

    /// <summary>
    /// Cấp độ log thông tin, dùng để ghi lại các hoạt động bình thường hoặc thông báo trạng thái trong ứng dụng.
    /// </summary>
    Info
}