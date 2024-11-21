using NClient.Core.Network;
using System.Text;
using TcpClientExample;

string serverIp = "192.168.1.2"; // IP của server
int serverPort = 65000;        // Port của server

TcpClientApp client = new(serverIp, serverPort);
client.Connect();

// Gửi dữ liệu dạng byte[] tới server
byte[] messageToSend = Encoding.UTF8.GetBytes("admin|1234");
Packet packet = new((byte)1, (short)2, messageToSend);
client.SendData(packet.ToByteArray());

// Giả lập một số thời gian đợi để nhận dữ liệu từ Server
Thread.Sleep(5000);
Console.ReadLine();
// Đóng kết nối
client.CloseConnection();