using System.Security.Authentication;

namespace NETServer.Infrastructure.Configuration
{
    public static class Setting
    {
        public readonly static byte VERSION = (byte)123;

        // Network Settings
        public readonly static int Port = NetworkConfig.Port;
        public readonly static string? IPAddress = NetworkConfig.IPAddress;
        public readonly static int MaxConnections = NetworkConfig.MaxConnections;
        public readonly static int BytesPerSecond = NetworkConfig.BytesPerSecond;
        public readonly static int MaxConnectionsPerIpAddress = NetworkConfig.MaxConnectionsPerIpAddress;
        public readonly static int RequestDelayMilliseconds = NetworkConfig.RequestDelayMilliseconds;
        public readonly static int ConnectionLockoutDuration = NetworkConfig.ConnectionLockoutDuration;
        public readonly static TimeSpan ClientSessionTimeout = NetworkConfig.ClientSessionTimeout;
        public readonly static (int MaxRequests, TimeSpan TimeWindow) RateLimit = NetworkConfig.RateLimit;

        // Các cài đặt mạng bổ sung
        public readonly static bool Blocking = NetworkConfig.Blocking;
        public readonly static bool KeepAlive = NetworkConfig.KeepAlive;
        public readonly static bool ReuseAddress = NetworkConfig.ReuseAddress;
        public readonly static int QueueSize = NetworkConfig.QueueSize;
        public readonly static int SendBuffer = NetworkConfig.SendBuffer;
        public readonly static int ReceiveBuffer = NetworkConfig.ReceiveBuffer;
        public readonly static int SendTimeout = NetworkConfig.SendTimeout;
        public readonly static int ReceiveTimeout = NetworkConfig.ReceiveTimeout;

        // Security Settings
        public readonly static bool IsSslEnabled = SecurityConfig.IsSslEnabled;
        public readonly static bool IsClientCertificateRequired = SecurityConfig.IsClientCertificateRequired;
        public readonly static bool IsCertificateRevocationCheckEnabled = SecurityConfig.IsCertificateRevocationCheckEnabled;
        public readonly static string SslPassword = SecurityConfig.SslPassword;
        public readonly static string SslPrivateKeyPath = SecurityConfig.SslPrivateKeyPath;
        public readonly static string SslCsrCertificatePath = SecurityConfig.SslCsrCertificatePath;
        public readonly static string SslCrtCertificatePath = SecurityConfig.SslCrtCertificatePath;
        public readonly static string SslPfxCertificatePath = SecurityConfig.SslPfxCertificatePath;
        public readonly static SslProtocols SupportedSslProtocols = SecurityConfig.SupportedSslProtocols;

        // RSA Settings
        public readonly static TimeSpan RsaKeyRotationInterval = SecurityConfig.RsaKeyRotationInterval;
        public readonly static string RsaPublicKeyFilePath = SecurityConfig.RsaPublicKeyFilePath;
        public readonly static string RsaPrivateKeyFilePath = SecurityConfig.RsaPrivateKeyFilePath;
        public readonly static string RsaShelfLifePath = SecurityConfig.RsaShelfLifePath;
    }
}
