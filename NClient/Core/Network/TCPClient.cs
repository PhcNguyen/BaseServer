using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpClientExample
{
    class TcpClientApp
    {
        private readonly string _serverIp;
        private readonly int _serverPort;
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private BinaryReader _reader;
        private BinaryWriter _writer;

        public TcpClientApp(string serverIp, int serverPort)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
        }

        // Kết nối đến Server
        public void Connect()
        {
            try
            {
                _tcpClient = new TcpClient(_serverIp, _serverPort);
                _networkStream = _tcpClient.GetStream();
                _reader = new BinaryReader(_networkStream);
                _writer = new BinaryWriter(_networkStream);

                Console.WriteLine("Connected to server...");
                StartReading();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while connecting to server: {ex.Message}");
            }
        }

        // Gửi dữ liệu tới Server
        public void SendData(byte[] data)
        {
            if (_tcpClient.Connected)
            {
                try
                {
                    _writer.Write(data);
                    Console.WriteLine($"Sent to server: {BitConverter.ToString(data)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while sending data: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Client is not connected to the server.");
            }
        }

        // Đọc dữ liệu từ Server
        private void StartReading()
        {
            Thread readThread = new Thread(new ThreadStart(ReadData));
            readThread.IsBackground = true;
            readThread.Start();
        }

        // Nhận dữ liệu từ Server dưới dạng byte[]
        private void ReadData()
        {
            try
            {
                while (_tcpClient.Connected)
                {
                    // Đọc dữ liệu từ stream dưới dạng byte
                    byte[] buffer = new byte[1024]; // Kích thước buffer có thể thay đổi tùy theo yêu cầu
                    int bytesRead = _reader.Read(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        byte[] dataReceived = new byte[bytesRead];
                        Array.Copy(buffer, dataReceived, bytesRead);
                        Console.WriteLine($"Received from server: {BitConverter.ToString(dataReceived)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while reading data: {ex.Message}");
            }
        }

        // Đóng kết nối
        public void CloseConnection()
        {
            try
            {
                _reader?.Dispose();
                _writer?.Dispose();
                _networkStream?.Close();
                _tcpClient?.Close();
                Console.WriteLine("Connection closed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while closing connection: {ex.Message}");
            }
        }


    }
}
