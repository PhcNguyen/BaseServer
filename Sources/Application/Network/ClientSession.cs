using NETServer.Infrastructure.Configuration;
using NETServer.Infrastructure.Interfaces;
using NETServer.Infrastructure.Security;
using NETServer.Infrastructure.Logging;

using System.Net;
using System.Text;
using System.Net.Sockets;


namespace NETServer.Application.Network;

internal class ClientSession
{
    private Stream? _clientStream;
    private DateTime _lastActivityTime;
    private readonly TcpClient _tcpClient;
    private readonly IRequestLimiter _requestLimiter;
    private readonly IConnectionLimiter _connectionLimiter;
    private readonly TimeSpan _sessionTimeout = Setting.ClientSessionTimeout;

    public readonly Guid Id = Guid.NewGuid();
    public bool IsConnected { get; private set; }
    public readonly byte[] KeyCipher = AesCipher.GenerateKey();
    public string ClientAddress { get; private set; } = string.Empty;

    public TcpClient TcpClient => _tcpClient;
    public Stream? ClientStream => _clientStream;

    public ClientSession(TcpClient tcpClient, IRequestLimiter requestLimiter, IConnectionLimiter connectionLimiter)
    {
        ValidateParameters(tcpClient, requestLimiter, connectionLimiter);

        _lastActivityTime = DateTime.UtcNow;

        _tcpClient = tcpClient;
        _requestLimiter = requestLimiter;
        _connectionLimiter = connectionLimiter;

        ClientAddress = ValidateClientAddress();
    }

    private static void ValidateParameters(TcpClient tcpClient, IRequestLimiter requestLimiter, IConnectionLimiter connectionLimiter)
    {
        ArgumentNullException.ThrowIfNull(tcpClient);
        ArgumentNullException.ThrowIfNull(requestLimiter);
        ArgumentNullException.ThrowIfNull(connectionLimiter);
    }

    private string ValidateClientAddress()
    {
        var clientEndPoint = _tcpClient.Client.RemoteEndPoint as IPEndPoint;
        return clientEndPoint?.Address.ToString() ?? string.Empty;
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

            _clientStream = Setting.IsSslEnabled
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

    public async Task Disconnect()
    {
        if (!IsConnected) return;
        if (!string.IsNullOrEmpty(ClientAddress))
        {
            await Task.Delay(0);
            _connectionLimiter.ConnectionClosed(ClientAddress);
        }

        try
        {
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
            await DataTransmitter.Send(_tcpClient, Encoding.UTF8.GetBytes("Client's endpoint is null or invalid."));
            await Disconnect();
            return false;
        }

        if (!_connectionLimiter.IsConnectionAllowed(ClientAddress))
        {
            await DataTransmitter.Send(_tcpClient, 
                Encoding.UTF8.GetBytes("Connection is denied due to max connections."));
            await Disconnect();
            return false;
        }

        if (!_requestLimiter.IsAllowed(ClientAddress))
        {
            await DataTransmitter.Send(_tcpClient, Encoding.UTF8.GetBytes("Request denied due to time limit."));
            await Disconnect();
            return false;
        }

        return true;
    }

    public void UpdateLastActivityTime() => _lastActivityTime = DateTime.UtcNow;

    public bool IsSessionTimedOut() => (DateTime.UtcNow - _lastActivityTime) > _sessionTimeout;
}
