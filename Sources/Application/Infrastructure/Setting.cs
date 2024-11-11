using System.Security.Authentication;

namespace NETServer.Application.Infrastructure;

internal class PathConfig
{
    // Cấu hình các thư mục chính trong dự án
    public readonly static string Base = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
    public readonly static string LogFolder = Path.Combine(Base, "Logs"); 
    public readonly static string ResourcesFolder = Path.Combine(Base, "Resources"); 
    public readonly static string SSLFolder = Path.Combine(ResourcesFolder, "SSL"); 
    public readonly static string RSAFolder = Path.Combine(ResourcesFolder, "RSA"); 
}

public static class Setting
{
    

    // Logging
    public readonly static bool IsDebug = true;  // Bật/tắt chế độ Debug
    public readonly static string LogDirectory = PathConfig.LogFolder;

    // Networks
    public readonly static string? IPAddress = null;  // Địa chỉ IP của server, null cho phép lắng nghe trên tất cả các địa chỉ
    public readonly static int Port = 65000;          // Cổng server lắng nghe kết nối
    public readonly static int MaxConnections = 5000; // Giới hạn kết nối tối đa đồng thời
    public readonly static TimeSpan SessionTimeout = TimeSpan.FromMinutes(10);

    // SSL Setup: To generate SSL certificate:
    // openssl genpkey -algorithm RSA -out server.key -pkeyopt rsa_keygen_bits:2048
    // openssl req -new -key server.key -out server.csr
    // openssl req -x509 -key server.key -in server.csr -out server.crt -days 365
    // openssl pkcs12 -export -out server.pfx -inkey server.key -in server.crt -certfile server.crt

    // Bật/Tắt SSL
    public readonly static bool UseSsl = false;  

    // Đường dẫn đến các tệp SSL
    public readonly static string SslPrivateKeyPath = Path.Combine(PathConfig.SSLFolder, "ssl_private.key");
    public readonly static string SslCsrCertificatePath = Path.Combine(PathConfig.SSLFolder, "ssl_certificate_request.csr");
    public readonly static string SslCrtCertificatePath = Path.Combine(PathConfig.SSLFolder, "ssl_certificate.crt");
    public readonly static string SslPfxCertificatePath = Path.Combine(PathConfig.SSLFolder, "ssl_certificate.pfx");
    public readonly static string SslCertificatePassword = "";  // Mật khẩu của chứng chỉ SSL

    public readonly static bool ClientCertificateRequired = false;   // Yêu cầu chứng chỉ từ client hay không
    public readonly static bool CheckCertificateRevocation = false;  // Kiểm tra chứng chỉ bị thu hồi hay không
    public readonly static SslProtocols EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;  // Các phiên bản TLS hỗ trợ

    // RSA
    public readonly static TimeSpan KeyRotationInterval = TimeSpan.FromDays(30); // Thời gian thay đổi khóa
    public readonly static string PublicKeyPath = Path.Combine(PathConfig.RSAFolder, "public-key.pem");
    public readonly static string PrivateKeyPath = Path.Combine(PathConfig.RSAFolder, "private-key.pem");
    public readonly static string ExpiryDatePath = Path.Combine(PathConfig.RSAFolder, "keyexpirydate.txt");



    // Security
    // Giới hạn số lần kết nối và cửa sổ thời gian tính giây
    public readonly static (int MaxRequests, TimeSpan TimeWindow) RequestLimit = (5, TimeSpan.FromSeconds(1));
    public readonly static int LockoutDuration = 300;  // Thời gian khóa kết nối (tính bằng giây)
    public readonly static int MaxConnectionsPerIp = 5;  // Giới hạn số kết nối tối đa từ một địa chỉ IP
}
