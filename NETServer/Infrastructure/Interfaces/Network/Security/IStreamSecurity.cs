using System.Net.Sockets;

namespace NETServer.Infrastructure.Interfaces
{
    /// <summary>
    /// Cung cấp các phương thức để thiết lập và quản lý kết nối SSL cho server.
    /// </summary>
    internal interface IStreamSecurity
    {
        /// <summary>
        /// Thiết lập kết nối SSL cho client.s
        /// Phương thức này sẽ xác thực server và mã hóa dữ liệu giữa client và server.
        /// </summary>
        /// <param name="tcpClient">Đối tượng TcpClient đại diện cho kết nối của client.</param>
        /// <returns>Trả về một <see cref="Stream"/> được mã hóa SSL để giao tiếp an toàn với client.</returns>
        /// <exception cref="InvalidOperationException">Ném ra nếu có lỗi trong quá trình xác thực SSL hoặc thiết lập kết nối.</exception>
        Task<Stream> EstablishSecureClientStream(TcpClient tcpClient);  
    }
}
