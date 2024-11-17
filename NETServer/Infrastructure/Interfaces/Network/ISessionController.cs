using System.Net.Sockets;

namespace NETServer.Infrastructure.Interfaces
{
    internal interface ISessionController
    {
        Task AcceptClient(TcpClient client);
        Task CloseAllConnections();
    }
}