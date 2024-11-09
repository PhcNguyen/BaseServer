using NETServer.Application.Security;
using NETServer.Infrastructure;
using NETServer.Logging;
using System.Net.Sockets;
using System.Net;

namespace NETServer.Application.NetSocketServer;

internal class ClientSession
{
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

    public async Task Connect()
    {
        try
        {
            if (string.IsNullOrEmpty(ClientAddress))
            {
                NLog.Warning("Client address is not set. Cannot establish connection.");
                return;
            }

            if (!_tcpClient.Connected)
            {
                NLog.Warning("TcpClient is not connected.");
                return;
            }

            this.IsConnected = true;

            _clientStream = Setting.UseSsl
                ? await SslSecurity.EstablishSecureClientStream(_tcpClient)
                : _tcpClient.GetStream();

            NLog.Info($"Session {Id} connected to {this.ClientAddress}");
        }
        catch (Exception ex)
        {
            NLog.Error($"Error connecting client {ClientAddress}: {ex.Message}");
            await DisconnectClient("Unknown", "Connection failed.");
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
            NLog.Warning("Client's endpoint is null or invalid.");
            await DisconnectClient("Unknown", "Invalid endpoint.");
            return false;
        }

        if (!await _connectionLimiter.IsConnectionAllowed(ClientAddress))
        {
            NLog.Warning($"Connection from {ClientAddress} is denied due to max connections.");
            await DisconnectClient(ClientAddress, "Max connections reached.");
            return false;
        }

        if (!await _requestLimiter.IsAllowed(ClientAddress))
        {
            NLog.Warning($"Request from {ClientAddress} is denied due to rate limit.");
            await DisconnectClient(ClientAddress, "Rate limit exceeded.");
            return false;
        }

        return true;
    }

    private async Task DisconnectClient(string clientIp, string reason)
    {
        try
        {
            await Disconnect();
            NLog.Info($"Disconnected client {clientIp} due to {reason}");
        }
        catch (Exception ex)
        {
            NLog.Error(ex, $"Error disconnecting client {clientIp}");
        }
    }
}
