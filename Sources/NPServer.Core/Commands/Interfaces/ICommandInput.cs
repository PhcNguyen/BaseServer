using NPServer.Common.Models;

namespace NPServer.Core.Commands.Interfaces;

/// <summary>
/// Giao diện đại diện cho dữ liệu đầu vào của một lệnh.
/// </summary>
public interface ICommandInput
{
    /// <summary>
    /// Gói dữ liệu (packet) được gửi cùng với lệnh.
    /// </summary>
    object Packet { get; }

    /// <summary>
    /// Lệnh được thực thi.
    /// </summary>
    Command Command { get; }

    /// <summary>
    /// Cấp độ truy cập (role) của người dùng đang thực thi lệnh.
    /// </summary>
    AccessLevel UserRole { get; }
}