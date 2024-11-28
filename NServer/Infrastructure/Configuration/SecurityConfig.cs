using System;
using System.IO;
using System.Security.Authentication;

namespace Base.Infrastructure.Configuration
{
    public static class SecurityConfig
    {
        // Cấu hình SSL
        // Sử dụng SSL cho các kết nối
        public readonly static bool IsSslEnabled = false;

        // Đường dẫn đến khóa riêng của SSL
        public readonly static string SslPrivateKeyPath = Path.Combine(PathConfig.SSLFolder, "ssl_private.key");

        // Đường dẫn đến chứng chỉ yêu cầu CSR (Certificate Signing Request)
        public readonly static string SslCsrCertificatePath = Path.Combine(PathConfig.SSLFolder, "ssl_certificate_request.csr");

        // Đường dẫn đến chứng chỉ SSL chính
        public readonly static string SslCrtCertificatePath = Path.Combine(PathConfig.SSLFolder, "ssl_certificate.crt");

        // Đường dẫn đến chứng chỉ SSL ở định dạng PFX
        public readonly static string SslPfxCertificatePath = Path.Combine(PathConfig.SSLFolder, "ssl_certificate.pfx");

        // Mật khẩu của chứng chỉ SSL
        public readonly static string SslPassword = "";

        // Kiểm tra yêu cầu chứng chỉ từ client (true nếu yêu cầu)
        public readonly static bool IsClientCertificateRequired = false;

        // Kiểm tra chứng chỉ bị thu hồi hay không
        public readonly static bool IsCertificateRevocationCheckEnabled = false;

        // Các phiên bản SSL/TLS mà server hỗ trợ
        public readonly static SslProtocols SupportedSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

        // Cấu hình RSA
        // Thời gian để thay đổi khóa RSA (Ví dụ: thay đổi sau 30 ngày)
        public readonly static TimeSpan RsaKeyRotationInterval = TimeSpan.FromDays(30);

        // Đường dẫn đến khóa công khai RSA
        public readonly static string RsaPublicKeyFilePath = Path.Combine(PathConfig.RSAFolder, "public-key.pem");

        // Đường dẫn đến khóa riêng RSA
        public readonly static string RsaPrivateKeyFilePath = Path.Combine(PathConfig.RSAFolder, "private-key.pem");

        // Đường dẫn đến tệp ghi lại ngày hết hạn của khóa RSA
        public readonly static string RsaShelfLifePath = Path.Combine(PathConfig.RSAFolder, "shelf-life.txt");
    }
}
