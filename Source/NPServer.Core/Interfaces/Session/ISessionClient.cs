using NPServer.Common.Models;
using NPServer.Infrastructure.Services;
using System;

namespace NPServer.Core.Interfaces.Session;

/// <summary>
/// Đại diện cho một phiên giao tiếp với client qua mạng.
/// </summary>
public interface ISessionClient
{
    /// <summary>
    /// ID duy nhất của phiên khách hàng.
    /// </summary>
    UniqueId Id { get; }

    /// <summary>
    /// Cấp độ truy cập của phiên.
    /// </summary>
    Authoritys Role { get; }

    /// <summary>
    /// Mạng kết nối của phiên.
    /// </summary>
    ISessionNetwork Network { get; }

    /// <summary>
    /// Khóa mã hóa của phiên.
    /// </summary>
    byte[] Key { get; }

    /// <summary>
    /// Kiểm tra xem phiên có đang kết nối hay không.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Địa chỉ IP của client đang kết nối.
    /// </summary>
    string EndPoint { get; }

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

    /// <summary>
    /// Cập nhật thời gian hoạt động gần nhất của phiên.
    /// </summary>
    void UpdateLastActivityTime();

    /// <summary>
    /// Kiểm tra xem phiên làm việc có hết thời gian chờ không.
    /// </summary>
    /// <returns>True nếu phiên làm việc đã hết thời gian chờ, ngược lại False.</returns>
    bool IsSessionTimedOut();

    /// <summary>
    /// Kiểm tra xem socket phiên có không hợp lệ hay không.
    /// </summary>
    bool IsSocketInvalid();

    /// <summary>
    /// Kết nối phiên làm việc.
    /// </summary>
    void Connect();

    /// <summary>
    /// Kết nối lại phiên làm việc.
    /// </summary>
    void Reconnect();

    /// <summary>
    /// Ngắt kết nối phiên làm việc.
    /// </summary>
    void Disconnect();

    /// <summary>
    /// Giải phóng tài nguyên của phiên làm việc.
    /// </summary>
    void Dispose();
}