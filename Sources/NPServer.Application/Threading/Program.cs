using NPServer.Application.Main;
using NPServer.Shared.Management;
using NPServer.Shared.Services;
using System;
using System.Threading;

namespace NPServer.Application.Threading;

internal static class Program
{
    private static readonly CancellationTokenSource _ctokens = new();
    private static readonly ServerApp _serverApp = Singleton.GetInstance<ServerApp>(() => new ServerApp(_ctokens));

    private static void Main()
    {
        Console.WriteLine(InfoOS.Details());
        Console.WriteLine(InfoCPU.Name());
        Console.WriteLine(InfoCPU.Usage());
        Console.WriteLine(InfoMemory.Usage());

        System.Console.ReadKey();

        System.Console.Title = $"NPServer ({ServiceController.VersionInfo})";

        ServiceController.Initialization();

        Thread.Sleep(1000);

        _serverApp.Run();

        System.Console.ReadKey();

        _serverApp.Shutdown();
    }
}