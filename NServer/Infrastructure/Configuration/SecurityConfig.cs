using System.IO;
using System.Security.Authentication;

namespace NServer.Infrastructure.Configuration
{
    /// <summary>
    /// Lớp cấu hình bảo mật, cung cấp các cấu hình liên quan đến SSL/TLS.
    /// </summary>
    public static class SecurityConfig
    {
        /// <summary>
        /// Cấu hình SSL.
        /// </summary>
        /// <value>True nếu SSL được kích hoạt, ngược lại False.</value>
        public static readonly bool IsSslEnabled = false;

        /// <summary>
        /// Đường dẫn đến khóa riêng của SSL.
        /// </summary>
        public static readonly string SslPrivateKeyPath = Path.Combine(PathConfig.SSLFolder, "ssl_private.key");

        /// <summary>
        /// Đường dẫn đến chứng chỉ yêu cầu CSR (Certificate Signing Request).
        /// </summary>
        public static readonly string SslCsrCertificatePath = Path.Combine(PathConfig.SSLFolder, "ssl_certificate_request.csr");

        /// <summary>
        /// Đường dẫn đến chứng chỉ SSL chính.
        /// </summary>
        public static readonly string SslCrtCertificatePath = Path.Combine(PathConfig.SSLFolder, "ssl_certificate.crt");

        /// <summary>
        /// Đường dẫn đến chứng chỉ SSL ở định dạng PFX.
        /// </summary>
        public static readonly string SslPfxCertificatePath = Path.Combine(PathConfig.SSLFolder, "ssl_certificate.pfx");

        /// <summary>
        /// Mật khẩu của chứng chỉ SSL.
        /// </summary>
        public static readonly string SslPassword = "";

        /// <summary>
        /// Kiểm tra yêu cầu chứng chỉ từ client.
        /// </summary>
        /// <value>True nếu yêu cầu chứng chỉ từ client, ngược lại False.</value>
        public static readonly bool IsClientCertificateRequired = false;

        /// <summary>
        /// Kiểm tra chứng chỉ bị thu hồi hay không.
        /// </summary>
        /// <value>True nếu kiểm tra chứng chỉ bị thu hồi, ngược lại False.</value>
        public static readonly bool IsCertificateRevocationCheckEnabled = false;

        /// <summary>
        /// Các phiên bản SSL/TLS mà server hỗ trợ.
        /// </summary>
        /// <value>Các phiên bản SSL/TLS được hỗ trợ.</value>
        public static readonly SslProtocols SupportedSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
    }
}