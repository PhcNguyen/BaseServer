using System.Net.Sockets;

namespace NETServer.Infrastructure.Interfaces
{
    internal interface ISessionController
    {
        void CleanUpInactiveSessions();
        ValueTask CloseAllConnections();
        Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken);
    }
}