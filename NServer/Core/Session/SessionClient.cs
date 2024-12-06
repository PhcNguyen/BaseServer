using NPServer.Core.Interfaces.Pooling;
using NPServer.Core.Interfaces.Session;
using NPServer.Core.Services;
using NPServer.Core.Session.Network;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace NPServer.Core.Session;

/// <summary>
/// Quản lý phiên làm việc của khách hàng.
/// <para>
/// Lớp này chịu trách nhiệm kết nối, xác thực, gửi/nhận dữ liệu và quản lý trạng thái của phiên làm việc từ phía khách hàng.
/// </para>
/// </summary>
/// <remarks>
/// Khởi tạo một đối tượng quản lý phiên làm việc.
/// </remarks>
/// <param name="socket">Socket kết nối của khách hàng.</param>
/// <param name="timeout">Thời gian chờ của phiên làm việc.</param>
/// <param name="multiSizeBuffer">Bộ đệm nhiều kích thước.</param>
/// <param name="token">Mã thông báo hủy để kiểm soát việc dừng giám sát.</param>
public class SessionClient(Socket socket, TimeSpan timeout, IMultiSizeBufferPool multiSizeBuffer, CancellationToken token) : ISessionClient
{
    private bool _isDisposed = false;
    private readonly UniqueId _id = UniqueId.NewId();
    private readonly CancellationToken _token = token;
    private readonly SessionConnection _connection = new(socket, timeout);
    private readonly SessionNetwork _network = new(socket, multiSizeBuffer);

    /// <summary>
    /// Kiểm tra trạng thái kết nối của phiên làm việc.
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// Địa chỉ IP của khách hàng.
    /// </summary>
    public string IpAddress => _connection.IpAddress;

    /// <summary>
    /// Khóa phiên làm việc (dành cho mã hóa hoặc xác thực).
    /// </summary>
    public byte[] Key { get; set; } = [];

    public bool Authenticator { get; set; } = false;

    /// <summary>
    /// Sự kiện cảnh báo.
    /// </summary>
    public event Action<string>? OnWarning;

    /// <summary>
    /// Sự kiện thông tin.
    /// </summary>
    public event Action<string>? OnInfo;

    /// <summary>
    /// Sự kiện lỗi.
    /// </summary>
    public event Action<string, Exception>? OnError;

    /// <summary>
    /// ID duy nhất của phiên làm việc.
    /// </summary>
    public UniqueId Id => _id;

    public SessionNetwork Network => _network;

    ISessionNetwork ISessionClient.Network => _network;

    /// <summary>
    /// Kết nối và bắt đầu xử lý dữ liệu từ khách hàng.
    /// </summary>
    public void Connect()
    {
        try
        {
            IsConnected = true;

            if (string.IsNullOrEmpty(_connection.IpAddress) || IsSocketInvalid())
            {
                OnWarning?.Invoke("Client address is invalid or Socket is not connected.");
                Disconnect();
                return;
            }

            _network.SocketReader.Receive(_token);

            OnInfo?.Invoke($"Session {_id} connected to {_connection.IpAddress}");
        }
        catch (Exception ex) when (ex is TimeoutException or IOException)
        {
            OnError?.Invoke($"Connection error for {_connection.IpAddress}", ex);
            Disconnect();
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Unexpected error for {_connection.IpAddress}", ex);
            Disconnect();
        }
    }

    public void Reconnect()
    {
        if (!IsConnected)
        {
            int retries = 3;
            TimeSpan delay = TimeSpan.FromSeconds(2);

            while (retries > 0)
            {
                try
                {
                    OnInfo?.Invoke("Đang thử kết nối lại...");
                    Connect();
                    return; // Kết nối thành công
                }
                catch (Exception ex)
                {
                    OnError?.Invoke($"Lần thử kết nối lại thất bại", ex);
                    retries--;
                    if (retries > 0)
                    {
                        Thread.Sleep(delay); // Tăng dần độ trễ
                        delay = delay.Add(delay); // Tăng gấp đôi độ trễ
                    }
                }
            }
        }
    }

    /// <summary>
    /// Ngắt kết nối phiên làm việc.
    /// </summary>
    public void Disconnect()
    {
        if (!IsConnected) return;

        IsConnected = false;

        try
        {
            Dispose();
            OnInfo?.Invoke($"Session {_id} disconnected from {_connection.IpAddress}");
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Error during disconnect", ex);
        }
    }

    /// <summary>
    /// Cập nhật thời gian hoạt động của phiên làm việc.
    /// </summary>
    public void UpdateLastActivityTime() => _connection.UpdateLastActivity();

    /// <summary>
    /// Kiểm tra xem phiên làm việc có hết thời gian chờ không.
    /// </summary>
    /// <returns>True nếu phiên làm việc đã hết thời gian chờ, ngược lại False.</returns>
    public bool IsSessionTimedOut() => _connection.IsTimedOut();

    /// <summary>
    /// Kiểm tra xem socket có hợp lệ hay không.
    /// </summary>
    public bool IsSocketInvalid() => _isDisposed || _network.IsDispose;

    /// <summary>
    /// Giải phóng tài nguyên của phiên làm việc.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;

        _network.Dispose();
        _connection.Dispose();

        _isDisposed = true;
    }
}