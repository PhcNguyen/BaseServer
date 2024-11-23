using System;
using System.IO;

namespace NServer.Infrastructure.Configuration
{
    internal class PathConfig
    {
        // Cấu hình các thư mục chính trong dự án
        public readonly static string Base = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
        public readonly static string LogFolder = Path.Combine(Base, "Logs");
        public readonly static string ResourcesFolder = Path.Combine(Base, "Resources");
        public readonly static string SSLFolder = Path.Combine(ResourcesFolder, "SSL");
        public readonly static string RSAFolder = Path.Combine(ResourcesFolder, "RSA");
    }
}
