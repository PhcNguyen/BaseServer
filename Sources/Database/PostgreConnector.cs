using Npgsql;

namespace NETServer.Database
{
    internal class PostgreConnector
    {
        public static NpgsqlConnection OpenConnection()
        {
            var connection = new NpgsqlConnection(PostgreConfig.ConnectionString);
            connection.Open();
            return connection;
        }

        public static bool TestConnection()
        {
            try
            {
                using var connection = OpenConnection();
                Console.WriteLine("Kết nối thành công!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi kết nối: {ex.Message}");
                return false;
            }
        }
    }
}
