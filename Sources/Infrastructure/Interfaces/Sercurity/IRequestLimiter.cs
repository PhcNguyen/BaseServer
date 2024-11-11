namespace NETServer.Infrastructure.Interfaces
{
    internal interface IRequestLimiter
    {
        bool IsAllowed(string ipAddress);
        Task ClearInactiveRequests();
        Task ClearBlockedIps();
    }
}
