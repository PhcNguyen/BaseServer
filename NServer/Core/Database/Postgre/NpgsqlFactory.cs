using NServer.Core.Interfaces.Database;

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
