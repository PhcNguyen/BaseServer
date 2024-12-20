using System;
using System.Linq;

namespace NPServer.Infrastructure.Helper;

public static class ConfigHelper
{
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

    public static TimeSpan ToTimeSpanFromSeconds(this int seconds) => TimeSpan.FromSeconds(seconds);
}