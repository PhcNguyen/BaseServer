using System;
using System.IO;
using System.Net.Sockets;

namespace NPClient.Core.Network
{
    internal class TCPCustom(string serverIp, int serverPort)
    {
        private readonly string _serverIp = serverIp;
        private readonly int _serverPort = serverPort;
        private TcpClient? _tcpClient;
        private NetworkStream? _networkStream;
        private BinaryReader? _reader;
        private BinaryWriter? _writer;

        public bool IsConnect = false;

        // Kết nối đến Server
        public void Connect()
        {
            try
            {
                _tcpClient = new TcpClient(_serverIp, _serverPort);
                _networkStream = _tcpClient.GetStream();
                _reader = new BinaryReader(_networkStream);
                _writer = new BinaryWriter(_networkStream);
                IsConnect = true;
            }
            catch (Exception ex)
            {
                IsConnect = false;
                throw new Exception($"Error while connecting to server: {ex.Message}");
            }
        }

        // Gửi dữ liệu tới Server
        public void SendData(byte[] data)
        {
            if (_tcpClient != null && _tcpClient.Connected)
            {
                try
                {
                    _writer?.Write(data);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error while sending data: {ex.Message}");
                }
            }
            else
            {
                throw new Exception("Client is not connected to the server.");
            }
        }

        public byte[] ReadData()
        {
            try
            {
                // Kiểm tra xem liệu kết nối có dữ liệu có sẵn không
                if (_tcpClient == null || !_tcpClient.Connected || _reader == null)
                {
                    return Array.Empty<byte>();  // Trả về mảng trống nếu không có kết nối hoặc không có reader
                }

                byte[] buffer = new byte[8192];
                int bytesRead = 0;

                // Kiểm tra nếu có dữ liệu sẵn để đọc từ stream
                if (_networkStream != null && _networkStream.DataAvailable)
                {
                    try
                    {
                        bytesRead = _reader.Read(buffer, 0, buffer.Length);
                    }
                    catch (IOException ex)
                    {
                        // Nếu kết nối bị đóng hoặc gián đoạn, bắt lỗi và thông báo
                        throw new Exception("Error while reading data: " + ex.Message);
                    }

                    if (bytesRead > 0)
                    {
                        byte[] dataReceived = new byte[bytesRead];
                        Array.Copy(buffer, dataReceived, bytesRead);

                        return dataReceived; // Trả về dữ liệu đã nhận
                    }
                }

                return Array.Empty<byte>();  // Nếu không có dữ liệu, trả về mảng trống
            }
            catch (Exception ex)
            {
                // Log lỗi hoặc xử lý lỗi chung
                throw new Exception($"Error while reading data: {ex.Message}");
            }
        }

        // Đóng kết nối
        public void CloseConnection()
        {
            IsConnect = false;
            try
            {
                _reader?.Dispose();
                _writer?.Dispose();
                _networkStream?.Close();
                _tcpClient?.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while closing connection: {ex.Message}");
            }
        }
    }
}