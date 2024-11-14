using Npgsql;

namespace NETServer.Database
{
    internal class PostgreConnector
    {
        public static async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(PostgreConfig.ConnectionString))
                throw new InvalidOperationException("Chuỗi kết nối không hợp lệ.");

            var _connection = new NpgsqlConnection(PostgreConfig.ConnectionString);
            await _connection.OpenAsync(cancellationToken);
            return _connection;
        }

        public static async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var connection = await OpenConnectionAsync(cancellationToken);
                return true;
            }
            catch (Exception)
            { 
                return false;
            }
        }
    }
}
