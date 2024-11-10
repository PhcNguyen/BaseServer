
namespace NETServer.Application.Security;

internal interface IConnectionLimiter
{
    Task<bool> IsConnectionAllowed(string ipAddress);
    Task ConnectionClosed(string ipAddress);
    int GetActiveConnections(string ipAddress);
}
