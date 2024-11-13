using NETServer.Database;

namespace NETServer
{
    class Program
    {
        private static void Main(string[] args)
        {
            PostgreConnector.TestConnection();
        }
    }

}
