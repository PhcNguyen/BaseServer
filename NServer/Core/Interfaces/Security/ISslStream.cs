using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NServer.Core.Interfaces.Security
{
    /// <summary>
    /// Cung cấp các phương thức để thiết lập và quản lý kết nối SSL cho server.
    /// </summary>
    internal interface ISslStream
    {
        ValueTask<Stream> EstablishSecureClientStreamAsync(TcpClient tcpClient);
        ValueTask<Stream> EstablishSecureClientStreamAsync(Socket socket);
    }
}
