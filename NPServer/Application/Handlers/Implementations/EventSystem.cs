using NPServer.Commands;
using NPServer.Models.Common;

namespace NPServer.Application.Handlers.Implementations;

/// <summary>
/// Lớp xử lý các lệnh hệ thống từ phía khách hàng.
/// </summary>
internal static class EventSystem
{
    /// <summary>
    /// Phương thức xử lý lệnh ping.
    /// </summary>
    /// <returns>Gói tin phản hồi với thông báo pong.</returns>
    [Command(Command.Ping, AccessLevel.Guests)]
    public static string Ping() => "Ping received. Server is responsive.";

    /// <summary>
    /// Phương thức xử lý lệnh pong.
    /// </summary>
    /// <returns>Gói tin phản hồi với thông báo ping.</returns>
    [Command(Command.Pong, AccessLevel.Guests)]
    public static string Pong() => "Pong received. Server is responsive.";

    /// <summary>
    /// Phương thức xử lý lệnh heartbeat.
    /// </summary>
    /// <returns>Gói tin phản hồi với thông báo thành công và trạng thái sống của server.</returns>
    [Command(Command.Heartbeat, AccessLevel.Guests)]
    public static string Heartbeat() => "Server is alive and operational.";

    /// <summary>
    /// Phương thức xử lý lệnh close.
    /// </summary>
    /// <returns>Gói tin rỗng để đóng kết nối.</returns>
    [Command(Command.Close, AccessLevel.Guests)]
    public static string Close() => string.Empty;
}