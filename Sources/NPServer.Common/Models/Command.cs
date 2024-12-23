namespace NPServer.Common.Models;

/// <summary>
/// Liệt kê các lệnh có thể được sử dụng trong hệ thống.
/// </summary>
public enum Command : short
{
    /// <summary>
    /// Không có lệnh.
    /// </summary>
    None = 0,

    /// <summary>
    /// Lệnh mặc định.
    /// </summary>
    Default,

    // Hệ thống cơ bản

    /// <summary>
    /// Lệnh kiểm tra kết nối (ping).
    /// </summary>
    Ping,

    /// <summary>
    /// Lệnh phản hồi kết nối (pong).
    /// </summary>
    Pong,

    /// <summary>
    /// Lệnh nhịp tim.
    /// </summary>
    Heartbeat,

    /// <summary>
    /// Lệnh đóng kết nối.
    /// </summary>
    Close,

    // Quản lý khóa

    /// <summary>
    /// Lệnh đặt khóa.
    /// </summary>
    SetKey,

    /// <summary>
    /// Lệnh lấy khóa.
    /// </summary>
    GetKey,

    /// <summary>
    /// Lệnh xóa khóa.
    /// </summary>
    DeleteKey,

    // Quản lý người dùng

    /// <summary>
    /// Lệnh đăng ký.
    /// </summary>
    Register,

    /// <summary>
    /// Lệnh đăng nhập.
    /// </summary>
    Login,

    /// <summary>
    /// Lệnh đăng xuất.
    /// </summary>
    Logout,

    /// <summary>
    /// Lệnh cập nhật mật khẩu.
    /// </summary>
    UpdatePassword,

    /// <summary>
    /// Lệnh xem hồ sơ.
    /// </summary>
    ViewProfile,

    /// <summary>
    /// Lệnh cập nhật hồ sơ.
    /// </summary>
    UpdateProfile,

    /// <summary>
    /// Lệnh xóa tài khoản.
    /// </summary>
    DeleteAccount,

    // Quản lý hệ thống

    /// <summary>
    /// Lệnh tắt hệ thống.
    /// </summary>
    Shutdown,

    /// <summary>
    /// Lệnh khởi động lại hệ thống.
    /// </summary>
    Restart,

    /// <summary>
    /// Lệnh kiểm tra trạng thái hệ thống.
    /// </summary>
    Status,

    // Kết quả xử lý

    /// <summary>
    /// Kết quả thành công.
    /// </summary>
    Success = 100,

    /// <summary>
    /// Kết quả lỗi.
    /// </summary>
    Error = 101,

    /// <summary>
    /// Lệnh không hợp lệ.
    /// </summary>
    InvalidCommand = 102,

    /// <summary>
    /// Thời gian chờ.
    /// </summary>
    Timeout = 103,
}
