using System;
using System.Threading.Tasks;

namespace NPServer.Core.Interfaces.Session;

/// <summary>
/// Giám sát và quản lý trạng thái của các phiên làm việc.
/// <para>
/// Lớp này chịu trách nhiệm giám sát các phiên làm việc, kiểm tra trạng thái kết nối của các
/// phiên làm việc và đóng các kết nối không hợp lệ hoặc đã hết thời gian.
/// </para>
/// </summary>
/// <remarks>
/// Khởi tạo một đối tượng giám sát phiên làm việc với bộ quản lý phiên và mã thông báo hủy.
/// </remarks>
public interface ISessionMonitor
{
    /// <summary>
    /// Sự kiện lỗi.
    /// </summary>
    public event Action<string, Exception>? ErrorOccurred;

    /// <summary>
    /// Theo dõi trạng thái các phiên khách hàng một cách bất kỳ.
    /// </summary>
    /// <returns>Task theo dõi các phiên khách hàng.</returns>
    Task MonitorSessionsAsync();

    /// <summary>
    /// Đóng kết nối của một phiên khách hàng cụ thể.
    /// </summary>
    /// <param name="session">Phiên khách hàng cần đóng kết nối.</param>
    void CloseConnection(ISessionClient session);
}
