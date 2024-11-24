using System.Net.Sockets;
using System.Threading.Tasks;

namespace NServer.Application.Interfaces.Main
{
    internal interface ISessionController
    {
        ValueTask AcceptClientAsync(TcpClient tcpClient);
        ValueTask CloseAllConnections();
    }
}