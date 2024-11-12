using System.Net.Sockets;

namespace NETServer.Infrastructure.Interfaces
{
    internal interface IStreamSecurity
    {
        // Thiết lập SSL cho kết nối
        Task<Stream> EstablishSecureClientStream(TcpClient tcpClient);  
    }
}
