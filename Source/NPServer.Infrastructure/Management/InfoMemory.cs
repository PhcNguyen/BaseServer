namespace NPServer.Infrastructure.Management;

/// <summary>
/// Lớp chứa các phương thức để lấy thông tin bộ nhớ.
/// </summary>
public static class InfoMemory
{
    /// <summary>
    /// Lấy thông tin về việc sử dụng bộ nhớ.
    /// </summary>
    /// <returns>Chuỗi mô tả trạng thái bộ nhớ hoặc thông báo lỗi.</returns>
    public static string Usage() =>
        SystemInfo.RunCommand("wmic OS get FreePhysicalMemory,TotalVisibleMemorySize /Value").ParseMemory();
}