using NServer.Core.Interfaces.Database;

namespace NServer.Core.Database.Postgre
{
    public class NpgsqlFactory : IDatabaseConnectionFactory
    {
        public IDatabaseConnection CreateConnection()
        {
            return new NpgsqlConnection();
        }
    }
}