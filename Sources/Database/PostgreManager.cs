using Npgsql;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NETServer.Database
{
    internal class PostgreManager
    {
        public async Task EnsureDatabaseExistsAsync(string database, CancellationToken cancellationToken = default)
        {
            await using var connection = await PostgreConnector.OpenConnectionAsync(cancellationToken);
            try
            {
                // Kiểm tra cơ sở dữ liệu đã tồn tại hay chưa (bất đồng bộ)
                using var cmd = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @dbName", connection);
                cmd.Parameters.AddWithValue("@dbName", database);
                var result = await cmd.ExecuteScalarAsync(cancellationToken);

                if (result == null)
                {
                    Console.WriteLine($"Cơ sở dữ liệu '{database}' không tồn tại. Đang tạo...");
                    await CreateDatabaseAsync(database, connection, cancellationToken);
                }
                else
                {
                    Console.WriteLine($"Cơ sở dữ liệu '{database}' đã tồn tại.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi kiểm tra/ tạo cơ sở dữ liệu: {ex.Message}");
            }
        }

        // Tạo cơ sở dữ liệu mới (bất đồng bộ)
        private async Task CreateDatabaseAsync(string database, NpgsqlConnection connection, CancellationToken cancellationToken = default)
        {
            using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", connection);
            await createCmd.ExecuteNonQueryAsync(cancellationToken);
            Console.WriteLine($"Cơ sở dữ liệu '{database}' đã được tạo thành công.");
        }

        public async Task<bool> ExecuteAsync(string query, CancellationToken cancellationToken = default, params object[] values)
        {
            if (string.IsNullOrEmpty(query) || values == null || values.Length == 0) return false;

            await using var connection = await PostgreConnector.OpenConnectionAsync(cancellationToken);
            try
            {
                using var cmd = new NpgsqlCommand(query, connection);

                // Lấy tên tham số từ câu truy vấn (những từ bắt đầu bằng '@')
                var parameterNames = query.Split(' ').Where(w => w.StartsWith("@")).Distinct().ToArray();

                // Kiểm tra số lượng tham số
                if (parameterNames.Length != values.Length)
                {
                    throw new ArgumentException($"Số lượng tham số truyền vào ({values.Length}) không khớp với số lượng tham số trong câu lệnh SQL ({parameterNames.Length}).");
                }

                // Thêm tham số vào câu lệnh SQL
                for (int i = 0; i < parameterNames.Length; i++)
                {
                    cmd.Parameters.AddWithValue(parameterNames[i], values[i]);
                }

                // Thực thi câu lệnh bất đồng bộ
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thực thi truy vấn: {ex.Message}");
                return false;
            }
        }
    }
}
