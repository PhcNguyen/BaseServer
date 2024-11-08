using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using NETServer.Logging;
using NETServer.Infrastructure;

namespace NETServer.Application.Security;

public class SslSecurity
{
    /// <summary>
    /// Thiết lập kết nối SSL cho client.
    /// </summary>
    /// <param name="client">Đối tượng TcpClient.</param>s
    /// <returns>Dòng dữ liệu mã hóa SSL.</returns>
    public static async Task<Stream> EstablishSecureClientStream(TcpClient client)
    {
        try
        {
            // Tạo SslStream từ NetworkStream của TcpClient
            SslStream sslStream = new SslStream(client.GetStream(), false);

            // Tải chứng chỉ của server
            var serverCertificate = new X509Certificate2(Setting.CertificatePath, Setting.CertificatePassword);

            // Xác thực server và bắt đầu mã hóa
            await sslStream.AuthenticateAsServerAsync(
                serverCertificate,
                clientCertificateRequired: false,
                enabledSslProtocols: System.Security.Authentication.SslProtocols.Tls12,
                checkCertificateRevocation: false
            );

            return sslStream;
        }
        catch (Exception ex)
        {
            NLog.Error($"Error during SSL authentication: {ex.Message}");
            client.Close();
            throw;
        }
    }
}
