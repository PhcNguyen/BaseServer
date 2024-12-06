using NPServer.Database.Interfaces;

namespace NPServer.Database.Postgre
{
    public class NpgsqlFactory : IDatabaseConnectionFactory
    {
        public IDatabaseConnection CreateConnection()
        {
            return new NpgsqlConnection();
        }
    }
}