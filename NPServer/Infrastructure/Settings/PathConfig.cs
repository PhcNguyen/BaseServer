using System;
using System.IO;

namespace NPServer.Infrastructure.Settings
{
    internal static class PathConfig
    {
        // Cấu hình các thư mục chính trong dự án
        public static readonly string Base = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) ?? "";

        public static readonly string LogFolder = Path.Combine(Base, "Logs");
        public static readonly string DataDirectory = Path.Combine(Base, "Data");
        public static readonly string ResourcesFolder = Path.Combine(Base, "Resources");

        /// <summary>
        /// Đảm bảo tất cả các thư mục được định nghĩa tồn tại.
        /// </summary>
        static PathConfig()
        {
            EnsureDirectoryExists(LogFolder);
            EnsureDirectoryExists(DataDirectory);
            EnsureDirectoryExists(ResourcesFolder);
        }

        /// <summary>
        /// Đảm bảo thư mục tồn tại, nếu chưa thì tạo mới.
        /// </summary>
        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}