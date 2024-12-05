using NServer.Application.Main;

namespace NServer.Application.Threading;

internal static class Program
{
    private static void Main(string[] args)
    {
        ServiceController.Register();
        ServiceController.Initialization();

        // Tạo instance của ServerEngine
        Server serverEngine = new();

        // Bắt đầu server
        serverEngine.StartServer();

        serverEngine.StopServer();
    }
}