namespace NPServer.Core.Interfaces.Session;

/// <summary>
/// Xử lý mạng cho phiên khách hàng, quản lý dữ liệu gửi/nhận.
/// </summary>
public interface ISessionNetwork
{
    /// <summary>
    /// Sự kiện khi dữ liệu được nhận.
    /// </summary>
    event System.Action<byte[]>? DataReceived;

    /// <summary>
    /// Sự kiện lỗi.
    /// </summary>
    event System.Action<string, System.Exception>? ErrorOccurred;

    /// <summary>
    /// Kiểm tra xem đối tượng đã được giải phóng hay chưa.
    /// </summary>
    bool IsDispose { get; }

    /// <summary>
    /// Gửi dữ liệu dưới dạng mảng byte.
    /// </summary>
    /// <param name="data">Dữ liệu cần gửi.</param>
    /// <returns>True nếu gửi thành công, ngược lại False.</returns>
    bool Send(byte[] data);

    /// <summary>
    /// Gửi dữ liệu dưới dạng chuỗi.
    /// </summary>
    /// <param name="data">Dữ liệu cần gửi dưới dạng chuỗi.</param>
    /// <returns>True nếu gửi thành công, ngược lại False.</returns>
    bool Send(string data);

    /// <summary>
    /// Giải phóng tài nguyên khi không còn sử dụng.
    /// </summary>
    void Dispose();
}
