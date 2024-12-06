using NPServer.Database.Interfaces;
using NPServer.Infrastructure.Configuration;
using NPServer.Infrastructure.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Database.Postgre
{
    public class NpgsqlConnection : IDatabaseConnection
    {
        private readonly Npgsql.NpgsqlConnection _connection;

        public NpgsqlConnection()
        {
            _connection = new Npgsql.NpgsqlConnection(SqlConfig.ConnectionString);
        }

        public void OpenConnection(CancellationToken cancellationToken = default)
        {
            try
            {
                _ = _connection.OpenAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                NPLog.Instance.Error("Failed to open connection to the PostgreSQL database.", ex);
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string query, params object[] parameters)
        {
            try
            {
                await using var cmd = new Npgsql.NpgsqlCommand(query, _connection);
                AddParameters(cmd, parameters);
                return await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                NPLog.Instance.Error($"Error executing non-query: {query}", ex);
                throw;
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(string query, params object[] parameters)
        {
            try
            {
                await using var cmd = new Npgsql.NpgsqlCommand(query, _connection);

                AddParameters(cmd, parameters);

                var result = await cmd.ExecuteScalarAsync();

                // Check if the result is DBNull or null and return default for nullable T
                if (result == DBNull.Value || result == null)
                {
                    throw new InvalidCastException($"Cannot convert DBNull or null to non-nullable type {typeof(T)}.");
                }

                // For non-nullable types, we need to safely cast the result
                return (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception ex)
            {
                NPLog.Instance.Error($"Error executing scalar query: {query}", ex);
                throw;
            }
        }

        private static void AddParameters(Npgsql.NpgsqlCommand command, object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                var parameter = new Npgsql.NpgsqlParameter($"@param{i}", values[i] ?? DBNull.Value);
                command.Parameters.Add(parameter);
            }
        }
    }
}