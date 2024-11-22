using System.Net.Sockets;

namespace NClient.Core.Network
{
    class TcpClientApp(string serverIp, int serverPort)
    {
        private readonly string _serverIp = serverIp;
        private readonly int _serverPort = serverPort;
        private TcpClient? _tcpClient;
        private NetworkStream? _networkStream;
        private BinaryReader? _reader;
        private BinaryWriter? _writer;

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
            if (_tcpClient != null && _tcpClient.Connected)
            {
                try
                {
                    if (_writer != null)
                    {
                        _writer.Write(data);
                        Console.WriteLine($"Sent to server: {BitConverter.ToString(data)}");
                    }
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
            Thread readThread = new Thread(new ThreadStart(ReadData))
            {
                IsBackground = true
            };
            readThread.Start();
        }

        // Nhận dữ liệu từ Server dưới dạng byte[]
        private void ReadData()
        {
            try
            {
                while (_tcpClient != null && _tcpClient.Connected && _reader != null)
                {
                    byte[] buffer = new byte[8192]; 
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
