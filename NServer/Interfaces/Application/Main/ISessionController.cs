using System.Net.Sockets;
using System.Threading.Tasks;

namespace NServer.Interfaces.Application.Main
{
    internal interface ISessionController
    {
        ValueTask AcceptClientAsync(TcpClient tcpClient);
        ValueTask CloseAllConnections();
    }
}