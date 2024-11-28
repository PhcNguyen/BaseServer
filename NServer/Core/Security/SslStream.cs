using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using Base.Core.Interfaces.Security;
using Base.Infrastructure.Configuration;

namespace Base.Core.Security
{
    /// <summary>
    /// Cung cấp các phương thức để thiết lập và quản lý kết nối SSL cho server.
    /// </summary>
    internal class SslStream : ISslStream
    {
        /// <summary>
        /// Thiết lập kết nối SSL cho client.
        /// </summary>
        /// <param name="tcpClient">Đối tượng TcpClient đại diện cho kết nối của client.</param>
        /// <returns>Trả về một <see cref="Stream"/> được mã hóa SSL.</returns>
        public async ValueTask<Stream> EstablishSecureClientStreamAsync(TcpClient tcpClient)
        {
            return await EstablishSecureStream(tcpClient.GetStream());
        }

        /// <summary>
        /// Thiết lập kết nối SSL cho client thông qua Socket.
        /// </summary>
        /// <param name="socket">Đối tượng Socket đại diện cho kết nối của client.</param>
        /// <returns>Trả về một <see cref="Stream"/> được mã hóa SSL.</returns>
        public async ValueTask<Stream> EstablishSecureClientStreamAsync(Socket socket)
        {
            return await EstablishSecureStream(new NetworkStream(socket, ownsSocket: false));
        }

        /// <summary>
        /// Phương thức chung để thiết lập SSLStream với các tham số cấu hình từ hệ thống.
        /// </summary>
        private static async ValueTask<Stream> EstablishSecureStream(Stream baseStream)
        {
            try
            {
                var sslStream = new System.Net.Security.SslStream(baseStream, leaveInnerStreamOpen: false);

                await sslStream.AuthenticateAsServerAsync(
                    serverCertificate: new X509Certificate2(Setting.SslPfxCertificatePath, Setting.SslPassword),
                    clientCertificateRequired: Setting.IsClientCertificateRequired,
                    checkCertificateRevocation: Setting.IsCertificateRevocationCheckEnabled,
                    enabledSslProtocols: Setting.SupportedSslProtocols
                ).ConfigureAwait(false);

                return sslStream;
            }
            catch (Exception ex) when (ex is AuthenticationException || ex is IOException || ex is SocketException)
            {
                throw new InvalidOperationException($"SSL connection setup failed: {ex.Message}", ex);
            }
        }
    }
}