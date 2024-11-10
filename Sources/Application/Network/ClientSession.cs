using NETServer.Application.Security;
using NETServer.Infrastructure;
using NETServer.Logging;

using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace NETServer.Application.Network;

internal class ClientSession
{
    private readonly ConcurrentDictionary<string, DateTime> _lastCooldown = new();
    private readonly ConnectionLimiter _connectionLimiter;
    private readonly RequestLimiter _requestLimiter;
    private readonly TcpClient _tcpClient;
    private Stream? _clientStream;

    public readonly Guid Id = Guid.NewGuid();
    public bool IsConnected { get; private set; }
    public string ClientAddress { get; private set; } = string.Empty;

    public TcpClient TcpClient => _tcpClient;
    public Stream? ClientStream => _clientStream;

    public ClientSession(TcpClient tcpClient, RequestLimiter requestLimiter, ConnectionLimiter connectionLimiter)
    {
        ValidateParameters(tcpClient, requestLimiter, connectionLimiter);

        _tcpClient = tcpClient;
        _requestLimiter = requestLimiter;
        _connectionLimiter = connectionLimiter;

        ClientAddress = ValidateClientAddress();
    }

    public static implicit operator TcpClient(ClientSession v)
    {
        if (v == null || v.TcpClient == null)
        {
            throw new InvalidOperationException("ClientSession or TcpClient is null.");
        }
        return v.TcpClient;
    }

    private static void ValidateParameters(TcpClient tcpClient, RequestLimiter requestLimiter, ConnectionLimiter connectionLimiter)
    {
        if (tcpClient == null) throw new ArgumentNullException(nameof(tcpClient), "TcpClient cannot be null");
        if (requestLimiter == null) throw new ArgumentNullException(nameof(requestLimiter), "RequestLimiter cannot be null");
        if (connectionLimiter == null) throw new ArgumentNullException(nameof(connectionLimiter), "ConnectionLimiter cannot be null");
    }

    private string ValidateClientAddress()
    {
        var clientEndPoint = _tcpClient.Client.RemoteEndPoint as IPEndPoint;
        return clientEndPoint?.Address.ToString() ?? string.Empty;
    }

    private void LogCooldown(string ipAddress, string message)
    {
        var cTime = DateTime.UtcNow;

        // Kiểm tra xem có thông báo từ IP này trong vòng 5 giây qua không
        if (_lastCooldown.TryGetValue(ipAddress, out var lastLoggedTime))
        {
            if ((cTime - lastLoggedTime).TotalSeconds < 10)
            {
                // Nếu đã có thông báo trong vòng 10 giây, không làm gì cả
                return;
            }
        }

        // In ra thông báo và cập nhật thời gian
        NLog.Warning($"Client {ipAddress}: {message}");
        _lastCooldown[ipAddress] = cTime;
    }

    public async Task Connect()
    {
        try
        {
            if (string.IsNullOrEmpty(ClientAddress))
            {
                NLog.Warning("Client address is not set. Cannot establish connection.");
                return;
            }

            // Kiểm tra kết nối trước khi lấy stream
            if (!_tcpClient.Connected)
            {
                NLog.Warning("TcpClient is not connected.");
                return;
            }

            // Lấy stream sau khi đã đảm bảo kết nối
            _clientStream = Setting.UseSsl
                ? await SslSecurity.EstablishSecureClientStream(_tcpClient)
                : _tcpClient.GetStream();

            // Kiểm tra lại kết nối sau khi lấy stream
            if (!_tcpClient.Connected)
            {
                NLog.Warning("TcpClient was disconnected after stream setup.");
                return;
            }

            // Đánh dấu là đã kết nối
            this.IsConnected = true;

            NLog.Info($"Session {Id} connected to {this.ClientAddress}");
        }
        catch (Exception ex)
        {
            // Chỉ thông báo lỗi nếu có ngoại lệ thực sự xảy ra
            NLog.Error($"Error connecting client {ClientAddress}: {ex.Message}");
            await Disconnect();
        }
    }

    public async Task Disconnect()
    {
        if (!IsConnected) return;

        try
        {
            if (!string.IsNullOrEmpty(ClientAddress))
            {
                await _connectionLimiter.ConnectionClosed(ClientAddress);
            }
            IsConnected = false;
            _clientStream?.Dispose();

            // Thông báo khi ngắt kết nối thành công
            NLog.Info($"Session {Id} disconnected from {ClientAddress}");
        }
        catch (Exception ex)
        {
            // Lỗi ngắt kết nối cũng sẽ được thông báo nếu có sự cố
            NLog.Error(ex, $"Error disconnecting client {ClientAddress}");
        }
    }

    public async Task<bool> AuthorizeClientSession()
    {
        if (string.IsNullOrEmpty(ClientAddress))
        {
            LogCooldown(ClientAddress, "Client's endpoint is null or invalid.");
            await Disconnect();
            return false;
        }

        if (!await _connectionLimiter.IsConnectionAllowed(ClientAddress))
        {
            LogCooldown(ClientAddress, $"Connection from {ClientAddress} is denied due to max connections.");
            await Disconnect();
            return false;
        }

        if (!await _requestLimiter.IsAllowed(ClientAddress))
        {
            LogCooldown(ClientAddress, $"Request from {ClientAddress} is denied due to rate limit.");
            await Disconnect();
            return false;
        }

        return true;
    }
}
