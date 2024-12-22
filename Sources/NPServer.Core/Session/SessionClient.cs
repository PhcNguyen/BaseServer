using NPServer.Core.Interfaces.Memory;
using NPServer.Core.Interfaces.Session;
using NPServer.Models.Common;
using NPServer.Shared.Services;
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
public sealed class SessionClient(Socket socket, TimeSpan timeout,
    IMultiSizeBufferPool multiSizeBuffer, CancellationToken token)
    : ISessionClient, IDisposable
{
    private bool _isDisposed = false;

    private readonly UniqueId _id = UniqueId.NewId();
    private readonly CancellationToken _token = token;
    private readonly SessionConnection _connection = new(socket, timeout);
    private readonly SessionNetwork _network = new(socket, multiSizeBuffer);

    /// <summary>
    /// ID duy nhất của phiên khách hàng.
    /// </summary>
    public UniqueId Id => _id;

    /// <summary>
    /// Cấp độ truy cập của phiên.
    /// </summary>
    public AccessLevel Role { get; private set; } = AccessLevel.Guests;

    /// <summary>
    /// Mạng kết nối của phiên.
    /// </summary>
    public SessionNetwork Network => _network;

    /// <summary>
    /// Khóa mã hóa của phiên.
    /// </summary>
    public byte[] Key { get; init; } = [];

    /// <summary>
    /// Kiểm tra xem phiên có đang kết nối hay không.
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// Địa chỉ IP của client đang kết nối.
    /// </summary>
    public string EndPoint => _connection.IpAddress;

    /// <summary>
    /// Sự kiện thông tin.
    /// </summary>
    public event Action<string>? InfoOccurred;

    /// <summary>
    /// Sự kiện cảnh báo.
    /// </summary>
    public event Action<string>? WarningOccurred;

    /// <summary>
    /// Sự kiện lỗi.
    /// </summary>
    public event Action<string, Exception>? ErrorOccurred;

    AccessLevel ISessionClient.Role => Role;

    ISessionNetwork ISessionClient.Network => _network;

    /// <summary>
    /// Cập nhật role của phiên.
    /// </summary>
    public void SetRole(AccessLevel newRole)
    {
        Role = newRole;
    }

    /// <summary>
    /// Cập nhật thời gian hoạt động gần nhất của phiên.
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
    /// Kết nối phiên làm việc.
    /// </summary>
    public void Connect()
    {
        try
        {
            IsConnected = true;

            if (string.IsNullOrEmpty(_connection.IpAddress) || IsSocketInvalid())
            {
                WarningOccurred?.Invoke("Client address is invalid or Socket is not connected.");
                Disconnect();
                return;
            }

            _network.SocketReader.Receive(_token);

            InfoOccurred?.Invoke($"Session {_id} connected to {_connection.IpAddress}");
        }
        catch (Exception ex) when (ex is TimeoutException or IOException)
        {
            ErrorOccurred?.Invoke($"Connection error for {_connection.IpAddress}", ex);
            Disconnect();
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke($"Unexpected error for {_connection.IpAddress}", ex);
            Disconnect();
        }
    }

    /// <summary>
    /// Kết nối lại phiên làm việc. 
    /// </summary>
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
                    InfoOccurred?.Invoke("Đang thử kết nối lại...");
                    Connect();
                    return; // Kết nối thành công
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke($"Lần thử kết nối lại thất bại", ex);
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
            _network.SocketReader?.Cancel();
            this.Dispose();
            InfoOccurred?.Invoke($"Session {_id} disconnected from {_connection.IpAddress}");
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke($"Error during disconnect", ex);
        }
    }

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