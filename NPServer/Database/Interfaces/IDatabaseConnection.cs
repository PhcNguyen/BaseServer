using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Database.Interfaces
{
    public interface IDatabaseConnection
    {
        void OpenConnection(CancellationToken cancellationToken = default);

        Task<int> ExecuteNonQueryAsync(string query, params object[] parameters);

        Task<T> ExecuteScalarAsync<T>(string query, params object[] parameters);
    }
}