namespace NETServer.Infrastructure;

internal class PathConfig
{
    public readonly static string Base = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
    public readonly static string LogFolder = Path.Combine(Base, "Logs");
    public readonly static string ResourcesFolder = Path.Combine(Base, "Resources");
    public readonly static string SSLFolder = Path.Combine(ResourcesFolder, "SSL");
    public readonly static string RSAFolder = Path.Combine(ResourcesFolder, "RSA");
}

public static class Setting
{
    

    // Logging
    public readonly static bool IsDebug = true;
    public readonly static string LogDirectory = PathConfig.LogFolder;

    // Networks
    public readonly static int Port = 65000;

    // SSL
    /* Create SSL
     * openssl genpkey -algorithm RSA -out server.key -pkeyopt rsa_keygen_bits:2048
     * openssl req -new -key server.key -out server.csr
     * openssl req -x509 -key server.key -in server.csr -out server.crt -days 365
     * openssl pkcs12 -export -out server.pfx -inkey server.key -in server.crt -certfile server.crt
     */
    public readonly static bool UseSsl = false;   // Cờ để bật/tắt SSL
    public readonly static string CertificatePath = Path.Combine(PathConfig.SSLFolder, "server.pfx");
    public readonly static string CertificatePassword = "";

    // RSA
    public readonly static TimeSpan KeyRotationInterval = TimeSpan.FromDays(365); // Thời gian thay đổi khóa
    public readonly static string PublicKeyPath = Path.Combine(PathConfig.RSAFolder, "public-key.pem");
    public readonly static string PrivateKeyPath = Path.Combine(PathConfig.RSAFolder, "private-key.pem");
    public readonly static string ExpiryDatePath = Path.Combine(PathConfig.RSAFolder, "keyexpirydate.txt");
    


    // Security
    public readonly static int Limit = 5;
    public readonly static int TimeWindow = 1;
    public readonly static int LockoutDuration = 300;
    public readonly static int MaxConnectionsPerIp = 5;
}
