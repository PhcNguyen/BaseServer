namespace NPServer.Database.Interfaces
{
    public interface IDatabaseConnectionFactory
    {
        IDatabaseConnection CreateConnection();
    }
}