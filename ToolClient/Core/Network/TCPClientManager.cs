namespace ToolClient.Core.Network
{
    public class TCPClientManager
    {
        private TCPCustom? _tcpClient;
        private readonly System.Windows.Forms.Timer _receiveTimer;
        private readonly System.Windows.Forms.Timer _sendPingTimer;

        public bool IsConnected => _tcpClient?.IsConnect ?? false;

        public event Action<string, Color, FontStyle>? ConsoleMessage;

        public TCPClientManager()
        {
            _receiveTimer = new System.Windows.Forms.Timer { Interval = 1000 }; // 1 giây
            _receiveTimer.Tick += (sender, e) => ReceiveData();

            _sendPingTimer = new System.Windows.Forms.Timer { Interval = 3000 }; // Gửi mỗi 3 giây
            _sendPingTimer.Tick += (sender, e) => SendPing();
        }

        public void Connect(string ip, int port)
        {
            try
            {
                _tcpClient = new TCPCustom(ip, port);
                _tcpClient.Connect();
                ConsoleMessage?.Invoke($"Kết nối đến IP: {ip} Port: {port}", Color.Green, FontStyle.Regular);
                _receiveTimer.Start();
                _sendPingTimer.Start();
            }
            catch (Exception ex)
            {
                ConsoleMessage?.Invoke($"Lỗi kết nối: {ex.Message}", Color.Red, FontStyle.Bold);
            }
        }

        public void Disconnect()
        {
            _receiveTimer.Stop();
            _sendPingTimer.Stop();

            _tcpClient?.CloseConnection();
            ConsoleMessage?.Invoke("Đã ngắt kết nối.", Color.Red, FontStyle.Regular);
        }

        public void SendData(byte[] data)
        {
            _tcpClient?.SendData(data);
            ConsoleMessage?.Invoke("Dữ liệu đã được gửi.", Color.Blue, FontStyle.Regular);
        }

        private void ReceiveData()
        {
            try
            {
                if (_tcpClient != null && _tcpClient.IsConnect)
                {
                    byte[] data = _tcpClient.ReadData();
                    if (data.Length > 0)
                    {
                        string receivedMessage = System.Text.Encoding.UTF8.GetString(data);
                        ConsoleMessage?.Invoke($"Dữ liệu nhận được từ server: {receivedMessage}", Color.Purple, FontStyle.Regular);
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleMessage?.Invoke($"Lỗi khi nhận dữ liệu: {ex.Message}", Color.Red, FontStyle.Italic);
            }
        }

        private void SendPing()
        {
            try
            {
                if (_tcpClient != null && _tcpClient.IsConnect)
                {
                    byte[] pingPacket = System.Text.Encoding.UTF8.GetBytes("pong");
                    Packet packet = new((byte)0, (sbyte)1, pingPacket);
                    _tcpClient.SendData(packet.ToByteArray());
                }
            }
            catch (Exception ex)
            {
                ConsoleMessage?.Invoke($"Lỗi khi gửi ping: {ex.Message}", Color.Red, FontStyle.Italic);
            }
        }
    }
}
