using NPServer.Application.Main;
using System.Threading;

namespace NPServer.Application.Threading
{
    internal static class Program
    {
        private static readonly CancellationTokenSource _ctokens = new();

        private static void Main()
        {
            System.Console.Title = $"NPServer ({ServiceController.VersionInfo})";

            ServiceController.RegisterSingleton();
            ServiceController.Initialization();

            Thread.Sleep(1000);

            ServerApp serverApp = new(_ctokens);

            serverApp.Run();

            System.Console.ReadKey();

            serverApp.Reset();

            System.Console.ReadKey();

            serverApp.Shutdown();
        }
    }
}