using NServer.Application.Threading;

using System;

namespace NServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Tạo instance của ServerEngine
            var serverEngine = new Server();

            // Bắt đầu server
            serverEngine.StartServer();

            Console.ReadKey();

            // Chuyển server về chế độ bảo trì
            serverEngine.SetMaintenanceMode(true);

            // Thoát chế độ bảo trì
            serverEngine.SetMaintenanceMode(false);

            // Reset lại server
            serverEngine.ResetServer();

            // Dừng server
            serverEngine.StopServer();
        }
    }
}