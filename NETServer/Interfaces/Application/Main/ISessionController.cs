using System.Net.Http;
using System.Net.Sockets;

namespace NETServer.Interfaces.Application.Main
{
    internal interface ISessionController
    {
        ValueTask AcceptClientAsync(TcpClient tcpClient);
        ValueTask CloseAllConnections();
    }
}