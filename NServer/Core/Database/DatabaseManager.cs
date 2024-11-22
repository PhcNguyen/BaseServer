using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Configuration;

using Npgsql;

namespace NServer.Core.Database
{
    internal class DatabaseManager
    {
        public static async ValueTask CreateTableAsync(string query, CancellationToken cancellationToken = default)
        {
            await using var connection = await NpgsqlConnection.OpenConnectionAsync(cancellationToken);

            try
            {
                await using var cmd = new NpgsqlCommand(query, connection);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Lỗi khi tạo bảng: {ex.Message}");
            }
        }

        // Tạo cơ sở dữ liệu mới
        public static async ValueTask CreateDatabaseAsync(string database, Npgsql.NpgsqlConnection connection, CancellationToken cancellationToken = default)
        {
            try
            {
                using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", connection);
                await createCmd.ExecuteNonQueryAsync(cancellationToken);
                Console.WriteLine($"Cơ sở dữ liệu '{database}' đã được tạo thành công.");
            }
            catch (Exception ex)
            {
                NLog.Error($"Lỗi khi tạo cơ sở dữ liệu: {ex.Message}");
            }
        }

        // Xóa cơ sở dữ liệu
        public static async ValueTask DropDatabaseAsync(string database, CancellationToken cancellationToken = default)
        {
            // Sử dụng ConnectionString từ config, thay đổi Database thành 'postgres' để thực hiện lệnh xóa cơ sở dữ liệu
            var connectionString = PostgreConfig.ConnectionString.Replace($"Database={PostgreConfig.DatabaseName};", "Database=postgres;");
            await using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            try
            {
                using var cmd = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{database}\"", connection);
                await cmd.ExecuteNonQueryAsync(cancellationToken);

                NLog.Info($"Cơ sở dữ liệu '{database}' đã được xóa.");
            }
            catch (Exception ex)
            {
                NLog.Error($"Lỗi khi xóa cơ sở dữ liệu", ex);
            }
        }

        public static async Task EnsureDatabaseExistsAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = await NpgsqlConnection.OpenConnectionAsync(cancellationToken);
            try
            {
                using var cmd = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @dbName", connection);
                cmd.Parameters.AddWithValue("@dbName", PostgreConfig.DatabaseName);
                var result = await cmd.ExecuteScalarAsync(cancellationToken);

                if (result == null)
                {
                    NLog.Info($"Cơ sở dữ liệu '{PostgreConfig.DatabaseName}' không tồn tại. Đang tạo...");

                    await CreateDatabaseAsync(PostgreConfig.DatabaseName, connection, cancellationToken);
                }
                else
                {
                    NLog.Info($"Cơ sở dữ liệu '{PostgreConfig.DatabaseName}' đã tồn tại.");
                }
            }
            catch (Exception ex)
            {
                NLog.Error($"Lỗi khi kiểm tra/ tạo cơ sở dữ liệu", ex);
            }
        }
    }
}