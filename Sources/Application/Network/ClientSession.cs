using NETServer.Application.Infrastructure;
using NETServer.Application.Security;
using NETServer.Logging;
using System.Net.Sockets;
using System.Net;

internal class ClientSession
{
    private Stream? _clientStream;
    private readonly TcpClient _tcpClient;
    private readonly RequestLimiter _requestLimiter;
    private readonly ConnectionLimiter _connectionLimiter;
    private readonly Dictionary<string, DateTime> _clientLastLogTimes;
    private readonly TimeSpan _sessionTimeout = Setting.SessionTimeout;

    public DateTime LastActivityTime;
    public readonly Guid Id = Guid.NewGuid();
    public bool IsConnected { get; private set; }
    public string ClientAddress { get; private set; } = string.Empty;

    public TcpClient TcpClient => _tcpClient;
    public Stream? ClientStream => _clientStream;

    public ClientSession(TcpClient tcpClient, RequestLimiter requestLimiter, ConnectionLimiter connectionLimiter, Dictionary<string, DateTime> clientLastLogTimes)
    {
        ValidateParameters(tcpClient, requestLimiter, connectionLimiter);

        LastActivityTime = DateTime.UtcNow;

        _tcpClient = tcpClient;
        _requestLimiter = requestLimiter;
        _connectionLimiter = connectionLimiter;
        _clientLastLogTimes = clientLastLogTimes;

        ClientAddress = ValidateClientAddress();
    }

    private static void ValidateParameters(TcpClient tcpClient, RequestLimiter requestLimiter, ConnectionLimiter connectionLimiter)
    {
        if (tcpClient == null) throw new ArgumentNullException(nameof(tcpClient));
        if (requestLimiter == null) throw new ArgumentNullException(nameof(requestLimiter));
        if (connectionLimiter == null) throw new ArgumentNullException(nameof(connectionLimiter));
    }

    private string ValidateClientAddress()
    {
        var clientEndPoint = _tcpClient.Client.RemoteEndPoint as IPEndPoint;
        return clientEndPoint?.Address.ToString() ?? string.Empty;
    }

    private void LogRateLimitedAction(string ipAddress, string message)
    {
        var cTime = DateTime.UtcNow;

        // Kiểm tra xem có thông báo từ IP này trong vòng _logCooldownTime qua không
        if (_clientLastLogTimes.TryGetValue(ipAddress, out var lastLoggedTime) &&
            (cTime - lastLoggedTime) < TimeSpan.FromSeconds(30)) return;


        // In ra thông báo và cập nhật thời gian
        NLog.Warning($"Client {ipAddress}: {message}");

        // Cập nhật thời gian sau khi thông báo
        _clientLastLogTimes[ipAddress] = cTime;
    }

    public async Task Connect()
    {
        try
        {
            if (string.IsNullOrEmpty(ClientAddress) || !_tcpClient.Connected)
            {
                NLog.Warning("Client address is invalid or TcpClient is not connected.");
                return;
            }

            _clientStream = Setting.UseSsl
                ? await SslSecurity.EstablishSecureClientStream(_tcpClient)
                : _tcpClient.GetStream();

            if (!_tcpClient.Connected)
            {
                NLog.Warning("TcpClient was disconnected after stream setup.");
                return;
            }

            IsConnected = true;
            NLog.Info($"Session {Id} connected to {ClientAddress}");
        }
        catch (Exception ex)
        {
            NLog.Error($"Error connecting client {ClientAddress}: {ex.Message}");
            await Disconnect();
        }
    }

    public void UpdateLastActivityTime() => LastActivityTime = DateTime.UtcNow;

    public bool IsSessionTimedOut() => (DateTime.UtcNow - LastActivityTime) > _sessionTimeout;

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
            NLog.Info($"Session {Id} disconnected from {ClientAddress}");
        }
        catch (Exception ex)
        {
            NLog.Error(ex, $"Error disconnecting client {ClientAddress}");
        }
    }

    public async Task<bool> AuthorizeClientSession()
    {
        if (string.IsNullOrEmpty(ClientAddress))
        {
            LogRateLimitedAction(ClientAddress, "Client's endpoint is null or invalid.");
            await Disconnect();
            return false;
        }

        if (!await _connectionLimiter.IsConnectionAllowed(ClientAddress))
        {
            LogRateLimitedAction(ClientAddress, $"Connection is denied due to max connections.");
            await Disconnect();
            return false;
        }

        if (!await _requestLimiter.IsAllowed(ClientAddress))
        {
            LogRateLimitedAction(ClientAddress, $"Request denied due to time limit.");
            await Disconnect();
            return false;
        }

        return true;
    }
}
