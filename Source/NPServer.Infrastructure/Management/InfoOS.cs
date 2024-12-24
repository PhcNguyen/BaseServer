using System.Runtime.InteropServices;

namespace NPServer.Infrastructure.Management;

/// <summary>
/// Lớp chứa các phương thức để lấy thông tin hệ điều hành.
/// </summary>
public static class InfoOS
{
    /// <summary>
    /// Lấy tên của hệ điều hành đang chạy.
    /// </summary>
    /// <returns>Chuỗi tên hệ điều hành hoặc "Unsupported OS" nếu không phải Windows.</returns>
    public static string GetOperatingSystem() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" : "Unsupported OS";

    /// <summary>
    /// Lấy thông tin chi tiết về hệ điều hành.
    /// </summary>
    /// <returns>Chuỗi mô tả chi tiết hệ điều hành hoặc thông báo lỗi.</returns>
    public static string Details() =>
        SystemInfo.RunCommand("wmic os get caption").ParseDefault();
}