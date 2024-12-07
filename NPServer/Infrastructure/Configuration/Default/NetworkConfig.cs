using NPServer.Infrastructure.Configuration.Abstract;
using System;

namespace NPServer.Infrastructure.Configuration.Default;

/// <summary>
/// Lớp cấu hình mạng, cung cấp các cấu hình liên quan đến kết nối mạng.
/// </summary>
internal class NetworkConfig : ConfigContainer
{
    /// <summary>
    /// Địa chỉ IP của server, null cho phép lắng nghe trên tất cả các địa chỉ IP có sẵn.
    /// </summary>
    public string IP { get; private set; } = "localhost";

    /// <summary>
    /// Cổng mà server sẽ lắng nghe kết nối (mặc định là 8080).
    /// </summary>
    public int Port { get; private set; } = 8080;

    /// <summary>
    /// Giới hạn số kết nối tối đa đồng thời mà server có thể xử lý.
    /// </summary>
    public int MaxConnections { get; private set; } = 100;

    /// <summary>
    /// Độ trễ (tính bằng millisecond) giữa các yêu cầu từ client đến server.
    /// </summary>
    public int RequestDelayMilliseconds { get; private set; } = 50;

    /// <summary>
    /// Thời gian khóa kết nối khi vượt quá giới hạn yêu cầu (300 giây).
    /// </summary>
    public int ConnectionLockoutDuration { get; private set; } = 300;

    /// <summary>
    /// Giới hạn số kết nối tối đa từ một địa chỉ IP (ví dụ: 20 kết nối từ cùng một IP).
    /// </summary>
    public int MaxConnectionsPerIpAddress { get; private set; } = 20;

    /// <summary>
    /// Tốc độ chậm (ví dụ: 512 KB/s, 1 MB/s) - Tốc độ cao (ví dụ: 5 MB/s, 10 MB/s).
    /// </summary>
    public int BytesPerSecond { get; private set; } = 524288;

    /// <summary>
    /// Chế độ không chặn.
    /// </summary>
    public bool Blocking { get; private set; } = false;

    /// <summary>
    /// Chế độ Keep-Alive cho kết nối.
    /// </summary>
    public bool KeepAlive { get; private set; } = true;

    /// <summary>
    /// Cho phép tái sử dụng địa chỉ.
    /// </summary>
    public bool ReuseAddress { get; private set; } = true;

    /// <summary>
    /// Thời gian phiên làm việc của client trước khi hết hạn (20 giây).
    /// </summary>
    [ConfigIgnore]
    public TimeSpan Timeout { get; private set; } = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Giới hạn yêu cầu tối đa trong một cửa sổ thời gian (ví dụ: 10 yêu cầu trong 0.1 giây).
    /// </summary>
    [ConfigIgnore]
    public (int MaxRequests, TimeSpan TimeWindow) RateLimit { get; private set; } = (10, TimeSpan.FromSeconds(0.1));
}