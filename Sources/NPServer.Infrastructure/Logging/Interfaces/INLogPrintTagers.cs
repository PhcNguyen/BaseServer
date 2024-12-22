using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPServer.Infrastructure.Logging.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho mục tiêu xử lý nhật ký.
/// </summary>
public interface INLogPrintTagers
{
    /// <summary>
    /// Thêm văn bản vào cuối nội dung hiện tại.
    /// </summary>
    /// <param name="text">Văn bản cần thêm.</param>
    void WriteLine(string text);
}
