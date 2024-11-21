using Npgsql;

namespace NServer.Core.Database
{
    internal partial class PostgreManager
    {
        /// <summary>
        /// Thực thi câu lệnh SQL bất đồng bộ với các tham số được cung cấp.
        /// </summary>
        /// <exception cref="ArgumentException">Ném ra khi câu lệnh query là null hoặc rỗng.</exception>
        /// <exception cref="ArgumentNullException">Ném ra khi tham số values là null.</exception>
        /// <remarks>
        /// - Các tham số sẽ được thêm vào câu lệnh với tên như @params0, @params1, v.v.
        /// </remarks>
        public static async ValueTask<bool> ExecuteAsync(string query, params object[] values)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));

            ArgumentNullException.ThrowIfNull(values);

            // Tạo kết nối và thực thi câu lệnh
            await using var connection = await PostgreConnector.OpenConnectionAsync();
            await using var cmd = new NpgsqlCommand(query, connection);

            try
            {
                // Thêm các tham số vào câu lệnh
                for (int i = 0; i < values.Length; i++)
                {
                    var paramName = $"@params{i}";
                    cmd.Parameters.AddWithValue(paramName, values[i] ?? DBNull.Value);
                }

                // Thực thi câu lệnh bất đồng bộ
                int affectedRows = await cmd.ExecuteNonQueryAsync();
                return affectedRows > 0; // Trả về true nếu có dòng bị ảnh hưởng
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi nếu có
                Console.Error.WriteLine($"Query execution failed: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        public static async ValueTask<T?> ExecuteScalarAsync<T>(string query, params object[] values)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));

            ArgumentNullException.ThrowIfNull(values);

            await using var connection = await PostgreConnector.OpenConnectionAsync();
            await using var cmd = new NpgsqlCommand(query, connection);

            try
            {
                // Thêm tham số vào câu lệnh
                for (int i = 0; i < values.Length; i++)
                {
                    var paramName = $"@param{i}";
                    cmd.Parameters.AddWithValue(paramName, values[i] ?? DBNull.Value);
                }

                // Thực thi câu lệnh và lấy giá trị trả về
                var result = await cmd.ExecuteScalarAsync();

                // Kiểm tra nếu result là null
                if (result == DBNull.Value || result == null)
                {
                    // Nếu kiểu T có thể nhận giá trị null, trả về default(T)
                    if (Nullable.GetUnderlyingType(typeof(T)) != null)
                    {
                        return default;
                    }

                    // Nếu không thể nhận null, ném ra lỗi
                    throw new InvalidCastException($"Cannot convert DBNull or null to type {typeof(T)}");
                }

                // Thực hiện chuyển đổi nếu kiểu trả về không phải null
                return (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Query execution failed: {ex.Message}\n{ex.StackTrace}");
                throw; // Ném lại exception để xử lý lỗi ở nơi gọi phương thức
            }
        }
    }
}
