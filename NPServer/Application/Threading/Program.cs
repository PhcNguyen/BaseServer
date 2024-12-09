using NPServer.Application.Main;
using NPServer.Infrastructure.Settings;
using NPServer.Tests;
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

            ServerApp serverApp = new(_ctokens);
            

            serverApp.Run();

            System.Console.ReadKey();

            serverApp.Reset();

            System.Console.ReadKey();

            serverApp.Shutdown();
        }
    }
}