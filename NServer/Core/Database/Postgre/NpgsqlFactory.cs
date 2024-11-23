using NServer.Interfaces.Core.Database;

namespace NServer.Core.Database.Postgre
{
    internal class NpgsqlFactory: IDatabaseConnectionFactory
    {
        public IDatabaseConnection CreateConnection()
        {
            return new NpgsqlConnection();
        }
    }
}
