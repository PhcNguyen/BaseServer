using NServer.Application.Main;
using NServer.Infrastructure.Logging;
using System;

namespace NServer.Application.Threading
{
    internal static class Program
    {
        private static void Initialization()
        {
            ServiceRegistry.RegisterServices();
            NLog.Instance.DefaultInitialization();
        }

        private static void Main(string[] args)
        {
            Initialization();
            // Tạo instance của ServerEngine
            Server serverEngine = new();

            // Bắt đầu server
            serverEngine.StartServer();

            Console.ReadKey();

            serverEngine.StopServer();
        }
    }
}