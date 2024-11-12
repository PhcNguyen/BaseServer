using NETServer.Infrastructure.Configuration;
using NETServer.Infrastructure.Interfaces;
using NETServer.Infrastructure.Services;
using NETServer.Infrastructure.Logging;

using System.Net;
using System.Text;
using System.Net.Sockets;



namespace NETServer.Application.Network;

internal class ClientSession: IClientSession
{
    private Stream? _clientStream;
    private DateTime _lastActivityTime;
    private readonly TcpClient _tcpClient;
    private readonly IStreamSecurity _streamSecurity;
    private readonly IRequestLimiter _requestLimiter;
    private readonly IConnectionLimiter _connectionLimiter;
    private readonly TimeSpan _sessionTimeout = Setting.ClientSessionTimeout;

    public Guid Id { get; private set; }
    public bool IsConnected { get; private set; }
    public byte[] SessionKey { get; private set; } 
    public string ClientAddress { get; private set; } = string.Empty;
    
    public TcpClient TcpClient => _tcpClient;
    public Stream? ClientStream => _clientStream;
    public DataTransmitter? DataTransport { get; private set; }

    public ClientSession(TcpClient tcpClient, IRequestLimiter requestLimiter, IConnectionLimiter connectionLimiter, IStreamSecurity streamSecurity)
    {
        ValidateParameters(tcpClient, requestLimiter, connectionLimiter, streamSecurity);

        _lastActivityTime = DateTime.UtcNow;

        _tcpClient = tcpClient;
        _requestLimiter = requestLimiter;
        _streamSecurity = streamSecurity;
        _connectionLimiter = connectionLimiter;

        ClientAddress = ValidateClientAddress();

        Id = Guid.NewGuid();
        SessionKey = Generator.K256();
    }

    private static void ValidateParameters(TcpClient tcpClient, IRequestLimiter requestLimiter, IConnectionLimiter connectionLimiter, IStreamSecurity streamSecurity)
    {
        ArgumentNullException.ThrowIfNull(tcpClient);
        ArgumentNullException.ThrowIfNull(streamSecurity);
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
                ? await _streamSecurity.EstablishSecureClientStream(_tcpClient)
                : _tcpClient.GetStream();

            if (!_tcpClient.Connected)
            {
                NLog.Warning("TcpClient was disconnected after stream setup.");
                return;
            }

            DataTransport = new DataTransmitter(_clientStream, SessionKey);

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

        try
        {
            IsConnected = false;
            _clientStream?.Dispose();

            NLog.Info($"Session {Id} disconnected from {ClientAddress}");

            if (!string.IsNullOrEmpty(ClientAddress))
            {
                _connectionLimiter.ConnectionClosed(ClientAddress);
            }
        }
        catch (Exception ex)
        {
            await Task.Delay(0);
            NLog.Error(ex);
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
