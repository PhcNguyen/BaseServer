using NServer.Application.Main;
using NServer.Core.Network.BufferPool;
using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Services;
using System;

namespace NServer.Application.Threading
{
    internal static class Program
    {
        private static void LogBufferInfo(MultiSizeBuffer multiSizeBuffer, int bufferSize)
        {
            multiSizeBuffer.GetPoolInfo(bufferSize, out int free, out int total, out int bufferSizeOut, out int misses);
            Console.WriteLine($"Buffer Size: {bufferSizeOut}");
            Console.WriteLine($"Total Buffers: {total}");
            Console.WriteLine($"Free Buffers: {free}");
            Console.WriteLine($"Misses: {misses}");
        }

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

            Console.ReadKey();

            MultiSizeBuffer _multiSizeBuffer = Singleton.GetInstance<MultiSizeBuffer>();

            LogBufferInfo(_multiSizeBuffer, 256);
        }
    }
}