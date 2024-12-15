using NPServer.Models.Common;

namespace NPServer.Commands.Interfaces;

public interface IRoleChecker
{
    bool HasAccess(AccessLevel role, Command command);
}
