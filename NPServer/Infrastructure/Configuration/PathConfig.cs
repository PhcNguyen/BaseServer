namespace NPServer.Infrastructure.Configuration;

internal static class PathConfig
{
    // Cấu hình các thư mục chính trong dự án
    public static readonly string Base = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory);

    public static readonly string LogFolder = System.IO.Path.Combine(Base, "Logs");
    public static readonly string ResourcesFolder = System.IO.Path.Combine(Base, "Resources");
    public static readonly string SSLFolder = System.IO.Path.Combine(ResourcesFolder, "SSL");
}