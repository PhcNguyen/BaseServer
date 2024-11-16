using Npgsql;
using System.Text.RegularExpressions;

namespace NETServer.Database
{
    internal partial class PostgreManager
    {
        // Kiểm tra cơ sở dữ liệu đã tồn tại hay chưa
        public static async Task EnsureDatabaseExistsAsync(string database, CancellationToken cancellationToken = default)
        {
            await using var connection = await PostgreConnector.OpenConnectionAsync(cancellationToken);
            try
            {
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

        // Tạo cơ sở dữ liệu mới 
        private static async Task CreateDatabaseAsync(string database, NpgsqlConnection connection, CancellationToken cancellationToken = default)
        {
            using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", connection);
            await createCmd.ExecuteNonQueryAsync(cancellationToken);
            Console.WriteLine($"Cơ sở dữ liệu '{database}' đã được tạo thành công.");
        }

        // Xóa cơ sở dữ liệu
        public static async Task DropDatabaseAsync(string database, CancellationToken cancellationToken = default)
        {
            var connectionString = PostgreConfig.ConnectionString.Replace($"Database={PostgreConfig.DatabaseName};", "Database=postgres;");
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            using var cmd = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{database}\"", connection);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            Console.WriteLine($"Cơ sở dữ liệu '{database}' đã được xóa.");
        }

        // Thực thi câu lệnh SQL với tham số
        public static async Task<bool> ExecuteAsync(string query, params object[] values)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));

            ArgumentNullException.ThrowIfNull(values);

            // Thay giá trị null bằng DBNull.Value để tương thích với cơ sở dữ liệu
            var parameters = values.Select(value => value ?? DBNull.Value).ToArray();

            await using var connection = await PostgreConnector.OpenConnectionAsync();
            await using var cmd = new NpgsqlCommand(query, connection);

            try
            {
                // Tìm các tham số bắt đầu bằng '@' trong câu truy vấn
                var parameterNames = new List<string>();
                var words = query.Split([' ', ',', ';', '(', ')', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in words)
                {
                    if (!string.IsNullOrEmpty(word) && word.Equals(string.Concat("@", word.AsSpan(1)), StringComparison.Ordinal))
                    {
                        parameterNames.Add(word);
                    }
                }

                // Loại bỏ các tham số trùng lặp
                parameterNames = parameterNames.Distinct().ToList();

                // Kiểm tra số lượng tham số
                if (parameterNames.Count != parameters.Length)
                {
                    throw new ArgumentException($"Mismatch: {parameterNames.Count} parameter(s) " +
                        $"expected but {parameters.Length} value(s) provided.");
                }

                // Gán tham số và giá trị vào câu lệnh SQL
                for (int i = 0; i < parameterNames.Count; i++)
                {
                    cmd.Parameters.AddWithValue(parameterNames[i], parameters[i]);
                }

                // Thực thi câu lệnh bất đồng bộ
                int affectedRows = await cmd.ExecuteNonQueryAsync();
                return affectedRows > 0; // Trả về true nếu có dòng bị ảnh hưởng
            }
            catch (Exception ex)
            {
                // Ghi lỗi chi tiết
                Console.Error.WriteLine($"Query execution failed: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }
    }
}
