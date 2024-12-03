using NServer.Core.Helpers;
using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace NServer.Core.Session.Utils;

/// <summary>
/// Quản lý kết nối socket của khách hàng và theo dõi thời gian hoạt động.
/// </summary>
public class SessionConnection(Socket socket, TimeSpan timeout)
{
    private readonly Socket _socket = socket ?? throw new ArgumentNullException(nameof(socket));
    private readonly TimeSpan _timeout = timeout;
    private readonly Stopwatch _activityTimer = Stopwatch.StartNew();
    private readonly string _clientIp = NetworkHelper.GetClientIP(socket);

    /// <summary>
    /// Kiểm tra trạng thái kết nối của phiên làm việc.
    /// </summary>
    public bool IsConnected => _socket.Connected;

    /// <summary>
    /// Địa chỉ IP của khách hàng.
    /// </summary>
    public string IpAddress => _clientIp;

    /// <summary>
    /// Cập nhật thời gian hoạt động của phiên làm việc.
    /// </summary>
    public void UpdateLastActivity() => _activityTimer.Restart();

    /// <summary>
    /// Kiểm tra xem phiên làm việc có hết thời gian chờ không.
    /// </summary>
    /// <returns>True nếu phiên làm việc đã hết thời gian chờ, ngược lại False.</returns>
    public bool IsTimedOut() => _activityTimer.Elapsed > _timeout;

    /// <summary>
    /// Ngắt kết nối phiên làm việc.
    /// </summary>
    public void Dispose()
    {
        _socket?.Close();
        _socket?.Dispose();
    }
}