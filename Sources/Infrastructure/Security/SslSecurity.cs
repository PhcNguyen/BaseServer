using NETServer.Infrastructure.Configuration;
using NETServer.Infrastructure.Interfaces;

using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;


namespace NETServer.Infrastructure.Security
{
    internal class SslSecurity: IStreamSecurity
    {
        /// <summary>
        /// Thiết lập kết nối SSL cho client.
        /// </summary>
        /// <param name="client">Đối tượng TcpClient.</param>
        /// <returns>Dòng dữ liệu mã hóa SSL.</returns>
        public async Task<Stream> EstablishSecureClientStream(TcpClient client)
        {
            try
            {
                // Tạo SslStream từ NetworkStream của TcpClient
                var sslStream = new SslStream(client.GetStream(), leaveInnerStreamOpen: false);

                // Xác thực server và bắt đầu mã hóa
                await sslStream.AuthenticateAsServerAsync(
                    serverCertificate: new X509Certificate2(Setting.SslPfxCertificatePath, Setting.SslPassword),
                    clientCertificateRequired: Setting.IsClientCertificateRequired,
                    checkCertificateRevocation: Setting.IsCertificateRevocationCheckEnabled,
                    enabledSslProtocols: Setting.SupportedSslProtocols
                );

                return sslStream;
            }
            catch (AuthenticationException authEx)
            {
                // Đóng client nếu có lỗi xác thực SSL
                client.Close();
                throw new InvalidOperationException("SSL authentication failed.", authEx);
            }
            catch (IOException ioEx)
            {
                // Xử lý lỗi kết nối mạng (ví dụ: thời gian chờ, mất kết nối)
                client.Close();
                throw new InvalidOperationException("Network error occurred during SSL setup.", ioEx);
            }
            catch (SocketException sockEx)
            {
                // Xử lý lỗi kết nối socket
                client.Close();
                throw new InvalidOperationException("Socket error occurred during SSL connection.", sockEx);
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi chung khác
                client.Close();
                throw new InvalidOperationException("SSL connection setup failed.", ex);
            }
        }
    }
}
