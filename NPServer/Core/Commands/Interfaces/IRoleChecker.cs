using NPServer.Models.Common;

namespace NPServer.Core.Commands.Interfaces;

public interface IRoleChecker
{
    bool HasAccess(AccessLevel role, Command command);
}