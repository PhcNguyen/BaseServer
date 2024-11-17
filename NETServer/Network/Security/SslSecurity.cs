using NETServer.Infrastructure.Configuration;
using NETServer.Infrastructure.Interfaces;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;


namespace NETServer.Network.Security
{
    /// <summary>
    /// Cung cấp các phương thức để thiết lập và quản lý kết nối SSL cho server.
    /// </summary>
    internal class SslSecurity : IStreamSecurity
    {
        /// <summary>
        /// Thiết lập kết nối SSL cho client.
        /// Phương thức này sẽ xác thực server và mã hóa dữ liệu giữa client và server.
        /// </summary>
        /// <param name="tcpClient">Đối tượng TcpClient đại diện cho kết nối của client.</param>
        /// <returns>Trả về một <see cref="Stream"/> được mã hóa SSL để giao tiếp an toàn với client.</returns>
        /// <exception cref="InvalidOperationException">Ném ra nếu có lỗi trong quá trình xác thực SSL hoặc thiết lập kết nối.</exception>
        public Stream EstablishSecureClientStream(TcpClient tcpClient)
        {
            try
            {
                // Tạo SslStream từ NetworkStream của TcpClient
                var sslStream = new SslStream(tcpClient.GetStream(), leaveInnerStreamOpen: false);

                // Xác thực server và bắt đầu mã hóa
                sslStream.AuthenticateAsServerAsync(
                    serverCertificate: new X509Certificate2(Setting.SslPfxCertificatePath, Setting.SslPassword),
                    clientCertificateRequired: Setting.IsClientCertificateRequired,
                    checkCertificateRevocation: Setting.IsCertificateRevocationCheckEnabled,
                    enabledSslProtocols: Setting.SupportedSslProtocols
                );

                return sslStream;
            }
            catch (Exception ex) when (ex is AuthenticationException || ex is IOException || ex is SocketException)
            {
                // Đóng client và ném ngoại lệ cụ thể với thông báo
                tcpClient.Close();
                throw new InvalidOperationException($"SSL connection setup failed: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi chung khác
                tcpClient.Close();
                throw new InvalidOperationException("SSL connection setup failed.", ex);
            }
        }
    }
}
