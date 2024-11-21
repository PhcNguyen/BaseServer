using System.Net.Http;
using System.Net.Sockets;

namespace NServer.Interfaces.Application.Main
{
    internal interface ISessionController
    {
        ValueTask AcceptClientAsync(TcpClient tcpClient);
        ValueTask CloseAllConnections();
    }
}