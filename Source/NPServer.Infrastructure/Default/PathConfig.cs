using System;
using System.IO;

namespace NPServer.Infrastructure.Default;

/// <summary>
/// Lớp cấu hình đường dẫn cho dự án.
/// </summary>
public static class PathConfig
{
    /// <summary>
    /// Đường dẫn cơ bản của dự án.
    /// </summary>
    public static readonly string Base = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) ?? "";

    /// <summary>
    /// Thư mục lưu trữ các file log.
    /// </summary>
    public static readonly string LogFolder = Path.Combine(Base, "Logs");

    /// <summary>
    /// Thư mục lưu trữ dữ liệu.
    /// </summary>
    public static readonly string DataDirectory = Path.Combine(Base, "Data");

    /// <summary>
    /// Thư mục lưu trữ tài nguyên.
    /// </summary>
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
    /// <param name="path">Đường dẫn tới thư mục cần kiểm tra.</param>
    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}
