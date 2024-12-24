namespace NPServer.Infrastructure.Management;

/// <summary>
/// Lớp chứa các phương thức để lấy thông tin CPU.
/// </summary>
public static class InfoCPU
{
    /// <summary>
    /// Lấy tên của CPU.
    /// </summary>
    /// <returns>Chuỗi tên CPU hoặc thông báo lỗi.</returns>
    public static string Name() =>
        SystemInfo.RunCommand("wmic cpu get name").ParseDefault();

    /// <summary>
    /// Lấy thông tin về phần trăm tải CPU.
    /// </summary>
    /// <returns>Chuỗi phần trăm tải CPU hoặc thông báo lỗi.</returns>
    public static string Usage() =>
        SystemInfo.RunCommand("wmic cpu get loadpercentage").ParseCPU();
}