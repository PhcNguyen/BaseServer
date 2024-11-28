using Base.Application.Threading;
using Base.Core.Network.BufferPool;
using Base.Infrastructure.Services;
using System;

namespace Base
{
    internal class Program
    {
        public static void LogBufferInfo(MultiSizeBuffer multiSizeBuffer, int bufferSize)
        {
            multiSizeBuffer.GetPoolInfo(bufferSize, out int free, out int total, out int bufferSizeOut, out int misses); 
            Console.WriteLine($"Buffer Size: {bufferSizeOut}"); 
            Console.WriteLine($"Total Buffers: {total}"); 
            Console.WriteLine($"Free Buffers: {free}"); 
            Console.WriteLine($"Misses: {misses}");
        }

        static void Main(string[] args)
        {
            // Tạo instance của ServerEngine
            var serverEngine = new Server();

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