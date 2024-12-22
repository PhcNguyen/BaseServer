namespace NPServer.Infrastructure.Logging.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho mục tiêu xử lý nhật ký.
/// </summary>
public interface INLogWinFormTagers
{
    /// <summary>
    /// Thêm văn bản vào cuối nội dung hiện tại.
    /// </summary>
    /// <param name="text">Văn bản cần thêm.</param>
    void AppendText(string text);

    /// <summary>
    /// Xóa toàn bộ văn bản hiện có.
    /// </summary>
    void ClearText();
}
