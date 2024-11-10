using System.Threading.Tasks;

namespace NETServer.Application.Security;

public interface IRequestLimiter
{
    Task<bool> IsAllowed(string ipAddress);
    Task ClearInactiveRequests();
    Task ClearBlockedIps();
}
