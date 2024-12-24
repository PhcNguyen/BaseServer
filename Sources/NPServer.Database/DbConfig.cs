using NPServer.Infrastructure.Configuration;

namespace NPServer.Database;

/// <summary>
/// Lớp cấu hình cho cơ sở dữ liệu.
/// </summary>
public class DbConfig : ConfigContainer
{
    /// <summary>
    /// Địa chỉ IP của máy chủ cơ sở dữ liệu.
    /// </summary>
    public string Host = "127.0.0.1";

    /// <summary>
    /// Cổng kết nối tới cơ sở dữ liệu.
    /// </summary>
    public int Port = 3306;

    /// <summary>
    /// Tên người dùng để kết nối tới cơ sở dữ liệu.
    /// </summary>
    public string User = "root";

    /// <summary>
    /// Mật khẩu để kết nối tới cơ sở dữ liệu.
    /// </summary>
    public string Password = "root";

    /// <summary>
    /// Tên của cơ sở dữ liệu.
    /// </summary>
    public string DbName = "NPS";
}