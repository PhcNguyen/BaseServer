using NPServer.Shared.Default;
using System;
using System.IO;

namespace NPServer.Infrastructure.Logging;

/// <summary>
/// Lớp tĩnh cung cấp các cấu hình cho hệ thống ghi nhật ký.
/// </summary>
public static class NPLogCongfig
{
    /// <summary>
    /// Đường dẫn tới thư mục ghi nhật ký, được tạo dựa trên ngày hiện tại.
    /// </summary>
    /// <remarks>
    /// Thư mục nhật ký được đặt trong thư mục cấu hình chung,
    /// cụ thể là trong <see cref="PathConfig.LogFolder"/> với định dạng "yyMMdd".
    /// </remarks>
    public static readonly string LogDirectory = Path.Combine(PathConfig.LogFolder, DateTime.Now.ToString("yyMMdd"));
}