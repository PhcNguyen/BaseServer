namespace NServer.Core.Interfaces.Database
{
    public interface IDatabaseConnectionFactory
    {
        IDatabaseConnection CreateConnection();
    }
}