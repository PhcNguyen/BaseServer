namespace NServer.Infrastructure.Configuration;

/// <summary>
/// Lớp cấu hình cho SQL, cung cấp các thông tin kết nối và cấu trúc bảng.
/// </summary>
internal static class SqlConfig
{
    /// <summary>
    /// Địa chỉ IP của server SQL.
    /// </summary>
    public static readonly string Host = "192.168.1.11";

    /// <summary>
    /// Tên người dùng để kết nối với SQL.
    /// </summary>
    public static readonly string Username = "ROOT";

    /// <summary>
    /// Mật khẩu để kết nối với SQL.
    /// </summary>
    public static readonly string Password = "APNxH8x5a";

    /// <summary>
    /// Tên của cơ sở dữ liệu SQL.
    /// </summary>
    public static readonly string DatabaseName = "Server";

    /// <summary>
    /// Chuỗi kết nối tới cơ sở dữ liệu SQL.
    /// </summary>
    public static readonly string ConnectionString = $"Host={Host};Username=postgres;Password={Password};Database={DatabaseName};Pooling=true;Max Pool Size=100;Min Pool Size=5;CommandTimeout=10;";

    /// <summary>
    /// Lệnh SQL để tạo bảng tài khoản nếu chưa tồn tại.
    /// </summary>
    public static readonly string AccountTableSchema = @"
        CREATE TABLE IF NOT EXISTS account (
            id SERIAL PRIMARY KEY,
            email VARCHAR(255) UNIQUE NOT NULL,
            password VARCHAR(255) NOT NULL,
            ban BOOLEAN DEFAULT FALSE,
            role BOOLEAN DEFAULT FALSE,
            active BOOLEAN DEFAULT FALSE,
            last_login TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        );";
}