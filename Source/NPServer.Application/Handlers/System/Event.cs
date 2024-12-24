using NPServer.Common.Models;
using NPServer.Core.Commands;

namespace NPServer.Application.Handlers.System;

/// <summary>
/// Lớp xử lý các lệnh hệ thống từ phía khách hàng.
/// </summary>
internal static class Event
{
    /// <summary>
    /// Phương thức xử lý lệnh ping.
    /// </summary>
    /// <returns>Gói tin phản hồi với thông báo pong.</returns>
    [Command(Command.Ping, Authoritys.Guests)]
    public static string Ping() => "Ping received. CoServer is responsive.";

    /// <summary>
    /// Phương thức xử lý lệnh pong.
    /// </summary>
    /// <returns>Gói tin phản hồi với thông báo ping.</returns>
    [Command(Command.Pong, Authoritys.Guests)]
    public static string Pong() => "Pong received. CoServer is responsive.";

    /// <summary>
    /// Phương thức xử lý lệnh close.
    /// </summary>
    /// <returns>Gói tin rỗng để đóng kết nối.</returns>
    [Command(Command.Close, Authoritys.Guests)]
    public static string Close() => string.Empty;

    /// <summary>
    /// Phương thức xử lý lệnh heartbeat.
    /// </summary>
    /// <returns>Gói tin phản hồi với thông báo thành công và trạng thái sống của server.</returns>
    [Command(Command.Heartbeat, Authoritys.Guests)]
    public static string Heartbeat() => "CoServer is alive and operational.";
}