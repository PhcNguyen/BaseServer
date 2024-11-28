using Base.Core.Interfaces.Database;

namespace Base.Core.Database.Postgre
{
    internal class NpgsqlFactory: IDatabaseConnectionFactory
    {
        public IDatabaseConnection CreateConnection()
        {
            return new NpgsqlConnection();
        }
    }
}
