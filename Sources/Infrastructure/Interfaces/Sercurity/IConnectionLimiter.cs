namespace NETServer.Infrastructure.Interfaces
{
    internal interface IConnectionLimiter
    {
        bool IsConnectionAllowed(string ipAddress);
        void ConnectionClosed(string ipAddress);
    }
}
