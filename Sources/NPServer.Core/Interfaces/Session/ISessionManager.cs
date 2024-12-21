using NPServer.Shared.Services;
using System.Collections.Generic;

namespace NPServer.Core.Interfaces.Session;

/// <summary>
/// Quản lý các phiên làm việc của khách hàng.
/// <para>
/// Lớp này chịu trách nhiệm quản lý, thêm, xóa và lấy các session hiện tại của người dùng.
/// </para>
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Thêm session mới vào danh sách và cập nhật số lượng session.
    /// </summary>
    /// <param name="session">Phiên làm việc cần thêm.</param>
    /// <returns>Trả về <c>true</c> nếu session được thêm thành công, ngược lại là <c>false</c>.</returns>
    bool AddSession(ISessionClient session);

    /// <summary>
    /// Lấy session theo ID.
    /// </summary>
    /// <param name="sessionId">ID của session cần tìm.</param>
    /// <returns>Trả về phiên làm việc nếu tìm thấy, nếu không trả về <c>null</c>.</returns>
    ISessionClient? GetSession(UniqueId sessionId);

    /// <summary>
    /// Thử lấy session theo ID.
    /// </summary>
    /// <param name="sessionId">ID của session cần tìm.</param>
    /// <param name="session">Session tìm thấy nếu có, hoặc <c>null</c> nếu không có.</param>
    /// <returns>Trả về <c>true</c> nếu tìm thấy session, ngược lại là <c>false</c>.</returns>
    bool TryGetSession(UniqueId sessionId, out ISessionClient? session);

    /// <summary>
    /// Xóa session theo ID.
    /// </summary>
    /// <param name="sessionId">ID của session cần xóa.</param>
    /// <returns>Trả về <c>true</c> nếu xóa thành công, ngược lại là <c>false</c>.</returns>
    bool RemoveSession(UniqueId sessionId);

    /// <summary>
    /// Lấy danh sách tất cả các session hiện tại.
    /// </summary>
    /// <returns>Trả về danh sách các session hiện tại.</returns>
    IEnumerable<ISessionClient> GetAllSessions();

    /// <summary>
    /// Lấy số lượng session hiện tại.
    /// </summary>
    /// <returns>Số lượng session hiện tại.</returns>
    int Count();
}
