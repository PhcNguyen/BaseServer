using System.Net.Sockets;

namespace NETServer.Interfaces.Core.Security
{
    /// <summary>
    /// Cung cấp các phương thức để thiết lập và quản lý kết nối SSL cho server.
    /// </summary>
    internal interface IStreamSecurity
    {
        ValueTask<Stream> EstablishSecureClientStreamAsync(TcpClient tcpClient);
        ValueTask<Stream> EstablishSecureClientStreamAsync(Socket socket);
    }
}
