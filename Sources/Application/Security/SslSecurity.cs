using NETServer.Infrastructure;

using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace NETServer.Application.Security;

public class SslSecurity
{
    /// <summary>
    /// Thiết lập kết nối SSL cho client.
    /// </summary>
    /// <param name="client">Đối tượng TcpClient.</param>
    /// <returns>Dòng dữ liệu mã hóa SSL.</returns>
    public static async Task<Stream> EstablishSecureClientStream(TcpClient client)
    {
        try
        {
            // Tạo SslStream từ NetworkStream của TcpClient
            var sslStream = new SslStream(client.GetStream(), leaveInnerStreamOpen: false);

            // Xác thực server và bắt đầu mã hóa
            await sslStream.AuthenticateAsServerAsync(
                // Tải chứng chỉ của server
                serverCertificate: new X509Certificate2(Setting.CertificatePath, Setting.CertificatePassword),
                clientCertificateRequired: Setting.ClientCertificateRequired,
                checkCertificateRevocation: Setting.CheckCertificateRevocation,
                enabledSslProtocols: Setting.EnabledSslProtocols
            );

            return sslStream;
        }
        catch (AuthenticationException authEx)
        {
            client.Close();
            throw new InvalidOperationException("SSL authentication failed.", authEx);
        }
        catch (Exception ex)
        {
            client.Close();
            throw new InvalidOperationException("SSL connection setup failed.", ex);
        }
    }
}