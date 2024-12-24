using NPServer.Common.Models;
using NPServer.Infrastructure.Configuration;
using NPServer.Infrastructure.Logging;

namespace NPServer.Database;

/// <summary>
/// Lớp quản lý kết nối và thao tác với cơ sở dữ liệu MySQL.
/// </summary>
public class SqlDb
{
    /// <summary>
    /// Cấu hình cơ sở dữ liệu.
    /// </summary>
    public static readonly DbConfig DbConfig = ConfigManager.Instance.GetConfig<DbConfig>();

    /// <summary>
    /// Đối tượng FreeSql để tương tác với cơ sở dữ liệu.
    /// </summary>
    public static IFreeSql FreeSql { get; }

    static SqlDb()
    {
        FreeSql = new FreeSql.FreeSqlBuilder()
            .UseConnectionString(global::FreeSql.DataType.MySql, $"Data Source={DbConfig.Host};Port={DbConfig.Port};User Id={DbConfig.User};Password={DbConfig.Password};")
            .UseAutoSyncStructure(true)
            .Build();

        // Kiểm tra cơ sở dữ liệu có tồn tại hay không
        var exists = FreeSql.Ado.QuerySingle<int>($"SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name = '{DbConfig.DbName}'") > 0;
        if (!exists)
        {
            FreeSql.Ado.ExecuteNonQuery($"CREATE DATABASE {DbConfig.DbName}");
            NPLog.Instance.Info<SqlDb>($"Database \"{DbConfig.DbName}\" không tồn tại, đã tự động tạo.");
        }

        // Kết nối lại
        FreeSql = new FreeSql.FreeSqlBuilder()
            .UseConnectionString(global::FreeSql.DataType.MySql, $"Data Source={DbConfig.Host};Port={DbConfig.Port};User Id={DbConfig.User};Password={DbConfig.Password};" +
                                                                  $"Initial Catalog={DbConfig.DbName};Charset=utf8;SslMode=none;Max pool size=10")
            .UseAutoSyncStructure(true)
            .Build();

        exists = FreeSql.DbFirst.GetTablesByDatabase(DbConfig.DbName).Exists(t => t.Name == "user");

        if (!exists)
        {
            // FreeSql.CodeFirst.SyncStructure<DbUser>();
            FreeSql.Insert(new DbUser("root", "1", Authoritys.Administrator)).ExecuteAffrows();
            NPLog.Instance.Info<SqlDb>($"Bảng \"user\" trong cơ sở dữ liệu \"{DbConfig.DbName}\" không tồn tại, đã tự động tạo và thêm một tài khoản quản trị (tài khoản=root, mật khẩu=1).");
        }
    }
}