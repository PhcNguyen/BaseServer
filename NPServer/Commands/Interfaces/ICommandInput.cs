using NPServer.Models.Common;

namespace NPServer.Commands.Interfaces;

public interface ICommandInput
{
    object Packet { get; }
    Command Command { get; }
    AccessLevel UserRole { get; }
}