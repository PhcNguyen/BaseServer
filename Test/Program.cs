using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPClientApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string serverIP = "192.168.1.2"; // Địa chỉ IP của server
            int port = 65000; // Cổng của server

            using TcpClient client = new TcpClient();
            try
            {
                Console.WriteLine("Đang kết nối đến server...");
                await client.ConnectAsync(serverIP, port);
                Console.WriteLine("Kết nối thành công!");

                using NetworkStream stream = client.GetStream();
                string message = "Xin chào, server!";
                byte[] data = Encoding.UTF8.GetBytes(message);

                Console.WriteLine("Đang gửi dữ liệu...");
                await stream.WriteAsync(data, 0, data.Length);
                Console.WriteLine("Dữ liệu đã được gửi!");

                // Nhận phản hồi từ server (nếu có)
                byte[] buffer = new byte[256];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Phản hồi từ server: " + response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
            }
        }
    }
}
