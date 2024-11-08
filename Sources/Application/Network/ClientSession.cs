using System.Net.Security;
using System.Net.Sockets;
using NETServer.Logging;
using NETServer.Application.Security;
using NETServer.Infrastructure;

namespace NETServer.Application.Network;

internal class ClientSession
{
    private Stream? _clientStream;
    private readonly TcpClient _tcpClient;

    public bool IsConnected { get; private set; }
    public string? ClientAddress { get; private set; }
    public Guid Id = Guid.NewGuid();

    public TcpClient TcpClient => _tcpClient;
    public Stream? NetworkStream => _clientStream;

    public ClientSession (TcpClient tcpClient) 
    {
        _tcpClient = tcpClient;
    }

    public static implicit operator TcpClient(ClientSession v)
    {
        throw new NotImplementedException();
    }

    public async Task Connect()
    {
        try
        {
            // Kiểm tra xem _tcpClient có được khởi tạo và kết nối không
            if (_tcpClient == null) return;

            this.IsConnected = true;

            // Sử dụng SslManager để xác định Stream phù hợp (SSL hoặc không)
            _clientStream = Setting.UseSsl
                ? await SslSecurity.EstablishSecureClientStream(_tcpClient)
                : _tcpClient.GetStream();

            // Kiểm tra xem địa chỉ client có hợp lệ không
            this.ClientAddress = _tcpClient.Client.RemoteEndPoint?.ToString();
            if (ClientAddress == null)
            {
                NLog.Warning("Failed to get client address.");
                await Disconnect();
                return;
            }

            NLog.Info($"Session {Id} connected to {ClientAddress}");
        }
        catch (SocketException error)
        {
            NLog.Error(error);
            await Disconnect();
        }
        catch (Exception error)
        {
            NLog.Error(error);
            await Disconnect();
        }
    }

    public async Task Disconnect()
    {
        if (!IsConnected) return;

        IsConnected = false; 

        try
        {
            // Đảm bảo tài nguyên được giải phóng
            await Task.Run(() => _clientStream?.Dispose()); 
            NLog.Info($"Session {Id} disconnected from {ClientAddress}");
        }
        catch (Exception error)
        {
            NLog.Error(error);
        }
    }
}