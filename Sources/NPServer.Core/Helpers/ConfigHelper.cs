using System;
using System.Linq;

namespace NPServer.Core.Helpers;

/// <summary>
/// Lớp helper cho các tác vụ liên quan đến Config.
/// </summary>
public static class ConfigHelper
{
    /// <summary>
    /// Phân tích một chuỗi chứa các cặp số nguyên và số thực thành mảng tuple (int, double).
    /// </summary>
    /// <param name="bufferAllocationsString">Chuỗi chứa các cặp số nguyên và số thực tách nhau bằng dấu phẩy và dấu chấm phẩy.</param>
    /// <returns>Mảng các tuple (int, double).</returns>
    public static (int, double)[] ParseBufferAllocations(this string bufferAllocationsString)
    {
        return bufferAllocationsString
            .Split(';') // Tách các cặp theo dấu chấm phẩy
            .Select(pair =>
            {
                var parts = pair.Split(','); // Tách mỗi cặp thành 2 phần
                return (int.Parse(parts[0].Trim()), double.Parse(parts[1].Trim())); // Chuyển đổi thành tuple
            })
            .ToArray(); // Chuyển đổi thành mảng
    }

    /// <summary>
    /// Chuyển đổi từ số giây thành một đối tượng TimeSpan.
    /// </summary>
    /// <param name="seconds">Số giây cần chuyển đổi.</param>
    /// <returns>Đối tượng TimeSpan đại diện cho số giây đã cho.</returns>
    public static TimeSpan ToTimeSpanFromSeconds(this int seconds) => TimeSpan.FromSeconds(seconds);
}
