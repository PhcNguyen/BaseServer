using System;

namespace NServer.Infrastructure.Configuration;

/// <summary>
/// Lớp cấu hình mạng, cung cấp các cấu hình liên quan đến kết nối mạng.
/// </summary>
internal static class NetworkConfig
{
    /// <summary>
    /// Địa chỉ IP của server, null cho phép lắng nghe trên tất cả các địa chỉ IP có sẵn.
    /// </summary>
    public static readonly string? IPAddress = null;

    /// <summary>
    /// Cổng mà server sẽ lắng nghe kết nối (mặc định là 65000).
    /// </summary>
    public static readonly int Port = 65000;

    /// <summary>
    /// Giới hạn số kết nối tối đa đồng thời mà server có thể xử lý.
    /// </summary>
    public static readonly int MaxConnections = 2000;

    /// <summary>
    /// Độ trễ (tính bằng millisecond) giữa các yêu cầu từ client đến server.
    /// </summary>
    public static readonly int RequestDelayMilliseconds = 50;

    /// <summary>
    /// Thời gian phiên làm việc của client trước khi hết hạn (20 giây).
    /// </summary>
    public static readonly TimeSpan ClientSessionTimeout = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Giới hạn yêu cầu tối đa trong một cửa sổ thời gian (ví dụ: 10 yêu cầu trong 0.1 giây).
    /// </summary>
    public static readonly (int MaxRequests, TimeSpan TimeWindow) RateLimit = (10, TimeSpan.FromSeconds(0.1));

    /// <summary>
    /// Thời gian khóa kết nối khi vượt quá giới hạn yêu cầu (300 giây).
    /// </summary>
    public static readonly int ConnectionLockoutDuration = 300;

    /// <summary>
    /// Giới hạn số kết nối tối đa từ một địa chỉ IP (ví dụ: 20 kết nối từ cùng một IP).
    /// </summary>
    public static readonly int MaxConnectionsPerIpAddress = 20;

    /// <summary>
    /// Tốc độ chậm (ví dụ: 512 KB/s, 1 MB/s) - Tốc độ cao (ví dụ: 5 MB/s, 10 MB/s).
    /// </summary>
    public static readonly int BytesPerSecond = 1048576 / 2;

    /// <summary>
    /// Chế độ không chặn.
    /// </summary>
    public static readonly bool Blocking = false;

    /// <summary>
    /// Chế độ Keep-Alive cho kết nối.
    /// </summary>
    public static readonly bool KeepAlive = true;

    /// <summary>
    /// Cho phép tái sử dụng địa chỉ.
    /// </summary>
    public static readonly bool ReuseAddress = true;
}