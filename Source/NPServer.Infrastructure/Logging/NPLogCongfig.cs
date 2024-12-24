using NPServer.Infrastructure.Default;
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
    /// </remarks>
    public static readonly string LogDirectory = Path.Combine(PathConfig.Base, DateTime.Now.ToString("yyMMdd"));
}