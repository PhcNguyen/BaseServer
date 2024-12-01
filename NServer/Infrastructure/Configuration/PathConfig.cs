using System;
using System.IO;

namespace NServer.Infrastructure.Configuration
{
    internal static class PathConfig
    {
        // Cấu hình các thư mục chính trong dự án
        public static readonly string Base = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);

        public static readonly string LogFolder = Path.Combine(Base, "Logs");
        public static readonly string ResourcesFolder = Path.Combine(Base, "Resources");
        public static readonly string SSLFolder = Path.Combine(ResourcesFolder, "SSL");
    }
}