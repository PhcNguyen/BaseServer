using NPServer.Application.Main;

namespace NPServer.Application.Threading
{
    internal static class Program
    {
        private static void Main()
        {
            System.Console.Title = $"NPServer ({ServerApp.VersionInfo})";

            ServiceController.RegisterSingleton();
            ServiceController.Initialization();

            ServerApp serverApp = new();

            serverApp.Run();

            System.Console.ReadKey();

            serverApp.Reset();

            System.Console.ReadKey();

            serverApp.Shutdown();
        }
    }
}