using System.Threading.Tasks;
using NServer.Core.Interfaces.Database;

namespace NServer.Core.Database
{
    internal partial class SqlExecutor(IDatabaseConnectionFactory connectionFactory)
    {
        private readonly IDatabaseConnectionFactory _connectionFactory = connectionFactory;

        public async Task<bool> ExecuteAsync(string query, params object[] parameters)
        {
            IDatabaseConnection connection = _connectionFactory.CreateConnection();
            connection.OpenConnection();

            return await connection.ExecuteNonQueryAsync(query, parameters) > 0;
        }

        public async Task<T> ExecuteScalarAsync<T>(string query, params object[] parameters)
        {
            IDatabaseConnection connection = _connectionFactory.CreateConnection();
            connection.OpenConnection();

            return await connection.ExecuteScalarAsync<T>(query, parameters);
        }

        public async ValueTask<bool> ExecuteAsync(SqlCommand command, params object[] values)
        {
            return await ExecuteAsync(SqlCommandMapper.Get(command), values);
        }

        // Phương thức ExecuteScalarAsync với SqlCommand
        public async ValueTask<T> ExecuteScalarAsync<T>(SqlCommand command, params object[] values)
        {
            return await ExecuteScalarAsync<T>(SqlCommandMapper.Get(command), values);
        }
    }
}