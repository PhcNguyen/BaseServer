using Npgsql;

namespace NETServer.Database
{
    internal class PostgreManager
    {
        public void EnsureDatabaseExists(string database)
        {
            using var connection = PostgreConnector.OpenConnection();
            try
            {
                // Kiểm tra cơ sở dữ liệu đã tồn tại hay chưa
                using var cmd = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = '{database}'", connection);
                var result = cmd.ExecuteScalar();
                if (result == null)
                {
                    Console.WriteLine($"Cơ sở dữ liệu '{database}' không tồn tại. Đang tạo...");
                    this.CreateDatabase(database, connection);
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
        private void CreateDatabase(string database, NpgsqlConnection connection)
        {
            using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", connection);

            createCmd.ExecuteNonQuery();
            Console.WriteLine($"Cơ sở dữ liệu '{database}' đã được tạo thành công.");
        }
    }
}
