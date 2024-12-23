namespace NPServer.Core.Interfaces.Session;

/// <summary>
/// Đại diện cho một kết nối phiên khách hàng với mạng.
/// </summary>
public interface ISessionConnection
{
    /// <summary>
    /// Kiểm tra xem kết nối có đang hoạt động hay không.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Địa chỉ IP của khách hàng.
    /// </summary>
    string IpAddress { get; }

    /// <summary>
    /// Cập nhật thời gian hoạt động cuối cùng của kết nối.
    /// </summary>
    void UpdateLastActivity();

    /// <summary>
    /// Thiết lập thời gian chờ cho kết nối.
    /// </summary>
    /// <param name="timeout">Thời gian chờ.</param>
    void SetTimeout(System.TimeSpan timeout);

    /// <summary>
    /// Kiểm tra xem phiên làm việc có hết thời gian chờ không.
    /// </summary>
    /// <returns>True nếu phiên làm việc đã hết thời gian chờ, ngược lại False.</returns>
    bool IsTimedOut();

    /// <summary>
    /// Giải phóng tài nguyên của kết nối.
    /// </summary>
    void Dispose();
}