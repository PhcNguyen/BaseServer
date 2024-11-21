using NServer.Infrastructure.Configuration;
using Npgsql;

namespace NServer.Core.Database
{
    internal class PostgreConnector : IAsyncDisposable
    {
        private NpgsqlConnection? _connection;

        public static async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(PostgreConfig.ConnectionString))
                throw new InvalidOperationException("Chuỗi kết nối không hợp lệ.");

            var connection = new NpgsqlConnection(PostgreConfig.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
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

        // Mở kết nối và lưu lại trong instance
        public async Task<NpgsqlConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
                return _connection;

            _connection = await OpenConnectionAsync(cancellationToken);
            return _connection;
        }

        // Xử lý giải phóng tài nguyên
        public async ValueTask DisposeAsync()
        {
            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
    }
}