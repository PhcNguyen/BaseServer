using NPServer.Core.Commands.Interfaces;
using NPServer.Common.Models;

namespace NPServer.Application.Handlers;

public sealed class CommandInput(object packet, Command command, Authoritys accessLevel)
    : ICommandInput
{
    // Gói tin từ client hoặc các dữ liệu liên quan
    public object Packet { get; } = packet;

    // Quyền truy cập của người dùng
    public Authoritys UserRole { get; } = accessLevel;

    // Lệnh cần xử lý
    Command ICommandInput.Command => command;
}