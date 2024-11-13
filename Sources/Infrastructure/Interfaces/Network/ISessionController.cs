using System.Net.Sockets;

namespace NETServer.Infrastructure.Interfaces
{
    internal interface ISessionController
    {
        void CleanUpInactiveSessions();
        Task CloseAllConnections();
        Task HandleClient(TcpClient client, CancellationToken cancellationToken);
    }
}