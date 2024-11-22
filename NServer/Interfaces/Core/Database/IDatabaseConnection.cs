namespace NServer.Interfaces.Core.Database
{
    internal interface IDatabaseConnection
    {
        Task<Npgsql.NpgsqlConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
    }
}
