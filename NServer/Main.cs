using NServer.Application.Threading;

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

            // Mô phỏng hành động sau khi server đã chạy (ví dụ: server chạy trong 10 giây rồi dừng)
            Console.WriteLine("Server is running... Press any key to stop.");
            Console.ReadKey();

            // Dừng server
            serverEngine.StopServer();

            // Chuyển server về chế độ bảo trì
            serverEngine.SetMaintenanceMode(true);
            Console.WriteLine("Server is in maintenance mode... Press any key to exit.");
            Console.ReadKey();

            // Thoát chế độ bảo trì
            serverEngine.SetMaintenanceMode(false);

            // Reset lại server
            serverEngine.ResetServer();
            Console.WriteLine("Server has been reset.");
        }
    }
}