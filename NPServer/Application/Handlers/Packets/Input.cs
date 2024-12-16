using NPServer.Commands.Interfaces;
using NPServer.Models.Common;

namespace NPServer.Application.Handlers.Packets;

public sealed class Input(object packet, Command command, AccessLevel accessLevel)
    : ICommandInput
{
    // Gói tin từ client hoặc các dữ liệu liên quan
    public object Packet { get; } = packet;

    // Quyền truy cập của người dùng
    public AccessLevel UserRole { get; } = accessLevel;

    // Lệnh cần xử lý
    Command ICommandInput.Command => command;
}