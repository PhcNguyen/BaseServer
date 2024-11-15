using NETServer.Infrastructure.Configuration;
using NETServer.Infrastructure.Interfaces;
using NETServer.Infrastructure.Services;
using NETServer.Infrastructure.Logging;
using NETServer.Application.Helper;

using System.Text;
using System.Net.Sockets;
using System.Diagnostics;

namespace NETServer.Application.Network
{
    internal class ClientSession: IClientSession, IDisposable
    {
        private Stream? _clientStream;
        private readonly TcpClient _tcpClient;
        private readonly Stopwatch _activityTimer;
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
        public DataTransmitter Transport { get; private set; }

        IDataTransmitter IClientSession.Transport => Transport;

        public ClientSession(TcpClient tcpClient, IRequestLimiter requestLimiter, 
            IConnectionLimiter connectionLimiter, IStreamSecurity streamSecurity)
        {
            ValidationHelper.EnsureNotNull(tcpClient, nameof(tcpClient));
            ValidationHelper.EnsureNotNull(streamSecurity, nameof(streamSecurity));
            ValidationHelper.EnsureNotNull(requestLimiter, nameof(requestLimiter));
            ValidationHelper.EnsureNotNull(connectionLimiter, nameof(connectionLimiter));

            _activityTimer = Stopwatch.StartNew();

            _tcpClient = tcpClient;
            _requestLimiter = requestLimiter;
            _streamSecurity = streamSecurity;
            _connectionLimiter = connectionLimiter;

            this.Id = Guid.NewGuid();
            this.SessionKey = Generator.K256();
            this.Transport = new DataTransmitter(Setting.BytesPerSecond);
            this.ClientAddress = ValidationHelper.GetClientAddress(_tcpClient);
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

                Transport.Create(_clientStream, SessionKey);

                this.IsConnected = true;
                NLog.Info($"Session {Id} connected to {ClientAddress}");
            }
            catch (TimeoutException tex)
            {
                NLog.Error($"Timeout while establishing connection for {ClientAddress}: {tex.Message}");
                await Disconnect();
            }
            catch (IOException ioex)
            {
                NLog.Error($"I/O error while setting up client stream for {ClientAddress}: {ioex.Message}");
                await Disconnect();
            }
            catch (Exception ex)
            {
                NLog.Error($"Unexpected error while connecting client {ClientAddress}: {ex.Message}");
                await Disconnect();
            }
        }

        public async Task Disconnect()
        {
            if (!this.IsConnected) return;

            try
            {
                this.IsConnected = false;

                if (!string.IsNullOrEmpty(ClientAddress))
                {
                    await Task.Delay(0);
                    _connectionLimiter.ConnectionClosed(ClientAddress);
                }

                this.Dispose();

                NLog.Info($"Session {Id} disconnected from {ClientAddress}");
            }
            catch (ObjectDisposedException ex)
            {
                NLog.Warning($"Attempted to dispose already disposed objects: {ex.Message}");
            }
            catch (Exception ex)
            {
                NLog.Error(ex);
            }
        }

        public void Dispose()
        {
            if (_clientStream != null)
            {
                _clientStream.Flush();
                _clientStream.Dispose();
                _clientStream = null;
            }

            _tcpClient.Dispose();
            Transport.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<bool> AuthorizeClientSession()
        {
            if (string.IsNullOrEmpty(this.ClientAddress))
            {
                await DataTransmitter.Send(_tcpClient, Encoding.UTF8.GetBytes("Client's endpoint is null or invalid."));
                await Disconnect();
                return false;
            }

            if (!_connectionLimiter.IsConnectionAllowed(this.ClientAddress))
            {
                await DataTransmitter.Send(_tcpClient, 
                    Encoding.UTF8.GetBytes("Connection is denied due to max connections."));
                await Disconnect();
                return false;
            }

            if (!_requestLimiter.IsAllowed(this.ClientAddress))
            {
                await DataTransmitter.Send(_tcpClient, Encoding.UTF8.GetBytes("Request denied due to time limit."));
                await Disconnect();
                return false;
            }

            return true;
        }

        public void UpdateLastActivityTime() => _activityTimer.Restart();

        public bool IsSessionTimedOut() => _activityTimer.Elapsed > _sessionTimeout;
    }
}