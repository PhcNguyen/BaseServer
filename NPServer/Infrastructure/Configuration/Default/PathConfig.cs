using System;
using System.IO;

namespace NPServer.Infrastructure.Configuration.Default;

internal static class PathConfig
{
    // Cấu hình các thư mục chính trong dự án
    public static readonly string Base = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) ?? "";

    public static readonly string LogFolder = Path.Combine(Base, "Logs");
    public static readonly string DataDirectory = Path.Combine(Base, "Data");
    public static readonly string ResourcesFolder = Path.Combine(Base, "Resources");
}