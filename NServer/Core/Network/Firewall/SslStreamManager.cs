using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace NServer.Core.Network.Firewall;

/// <summary>
/// Cung cấp các phương thức để thiết lập và quản lý kết nối SSL cho server.
/// </summary>
public class SslStreamManager(string filename, string password, bool clientCertificateRequired,
        bool checkCertificateRevocation, SslProtocols enabledSslProtocols)
{
    private readonly string _filename = filename;
    private readonly string? _password = password;
    private readonly bool _clientCertificateRequired = clientCertificateRequired;
    private readonly bool _checkCertificateRevocation = checkCertificateRevocation;
    private readonly SslProtocols _enabledSslProtocols = enabledSslProtocols;

    /// <summary>
    /// Thiết lập kết nối SSL cho client.
    /// </summary>
    /// <param name="tcpClient">Đối tượng TcpClient đại diện cho kết nối của client.</param>
    /// <returns>Trả về một <see cref="Stream"/> được mã hóa SSL.</returns>
    public async ValueTask<Stream> EstablishSecureClientStreamAsync(TcpClient tcpClient) =>
        await EstablishSecureStream(tcpClient.GetStream());
    
    /// <summary>
    /// Thiết lập kết nối SSL cho client thông qua Socket.
    /// </summary>
    /// <param name="socket">Đối tượng Socket đại diện cho kết nối của client.</param>
    /// <returns>Trả về một <see cref="Stream"/> được mã hóa SSL.</returns>
    public async ValueTask<Stream> EstablishSecureClientStreamAsync(Socket socket) =>
        await EstablishSecureStream(new NetworkStream(socket, ownsSocket: false));
    
    /// <summary>
    /// Phương thức chung để thiết lập SSLStream với các tham số cấu hình từ hệ thống.
    /// </summary>
    private async ValueTask<Stream> EstablishSecureStream(Stream baseStream)
    {
        try
        {
            System.Net.Security.SslStream sslStream = new(baseStream, leaveInnerStreamOpen: false);

            await sslStream.AuthenticateAsServerAsync(
                serverCertificate: X509Certificate2.CreateFromPemFile(_filename, _password),
                clientCertificateRequired: _clientCertificateRequired,
                checkCertificateRevocation: _checkCertificateRevocation,
                enabledSslProtocols: _enabledSslProtocols
            ).ConfigureAwait(false);

            return sslStream;
        }
        catch (Exception ex) when (ex is AuthenticationException || ex is IOException || ex is SocketException)
        {
            throw new InvalidOperationException($"SSL connection setup failed: {ex.Message}", ex);
        }
    }
}