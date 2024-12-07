using NPServer.Application.Main;
using System;

namespace NPServer.Application.Threading;

internal static class Program
{
    private static void Main(string[] args)
    {
        ServiceController.RegisterSingleton();
        ServiceController.Initialization();

        // Tạo instance của ServerEngine
        Server serverEngine = new();

        // Bắt đầu server
        serverEngine.StartServer();

        Console.ReadKey();

        serverEngine.StopServer();
    }
}