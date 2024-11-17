using NETServer.Infrastructure.Logging;
using NETServer.Infrastructure.Services;
using NETServer.Infrastructure.Interfaces;
using NETServer.Infrastructure.Configuration;

using System.Net.Sockets;
using System.Diagnostics;
using NETServer.Network.Packets;
using NETServer.Infrastructure.Helper;

namespace NETServer.Network
{
    internal class ClientSession : IClientSession, IDisposable
    {
        private readonly Guid _id;
        private Stream? _clientStream;
        private readonly TcpClient _tcpClient;
        private readonly ByteBuffer _byteBuffer;
        private readonly Stopwatch _activityTimer;
        private readonly IStreamSecurity _streamSecurity;
        private readonly IConnLimiter _connectionLimiter;
        private readonly TimeSpan _sessionTimeout = Setting.ClientSessionTimeout;
        private readonly PacketThrottles _throttles = new(Setting.BytesPerSecond);

        public bool IsConnected { get; private set; }
        public byte[] SessionKey { get; private set; }
        public string ClientAddress { get; private set; } = string.Empty;

        public Guid ID => _id;
        public TcpClient TcpClient => _tcpClient;
        public Stream? ClientStream => _clientStream;
        public DataTransmitter? Transport { get; private set; }

        Guid IClientSession.ID => _id;
        IDataTransmitter IClientSession.Transport => Transport ?? throw new InvalidOperationException("Transport is not initialized.");

        public ClientSession(TcpClient tcpClient, ByteBuffer bufferPool,
            IStreamSecurity streamSecurity, IConnLimiter connectionLimiter)
        {
            _activityTimer = Stopwatch.StartNew();

            _id = Guid.NewGuid();
            _tcpClient = tcpClient;
            _byteBuffer = bufferPool;
            _streamSecurity = streamSecurity;
            _connectionLimiter = connectionLimiter;

            SessionKey = Generator.K256();
            ClientAddress = Validator.GetClientAddress(_tcpClient);
        }

        private Stream SetupClientStream()
        {
            if (Setting.IsSslEnabled)
            {
                return _streamSecurity.EstablishSecureClientStream(_tcpClient);
            }
            return _tcpClient.GetStream();
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

                _clientStream = SetupClientStream();

                if (!_tcpClient.Connected)
                {
                    NLog.Warning("TcpClient was disconnected after stream setup.");
                    return;
                }

                Transport = new DataTransmitter(_id, _clientStream, _byteBuffer, _throttles);

                IsConnected = true;
                NLog.Info($"Session {_id} connected to {ClientAddress}");
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
            if (!IsConnected) return;

            try
            {
                IsConnected = false;

                if (!string.IsNullOrEmpty(ClientAddress))
                {
                    _connectionLimiter.ConnectionClosed(ClientAddress);
                }

                await Task.Run(() => Dispose());

                NLog.Info($"Session {_id} disconnected from {ClientAddress}");
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

            Transport?.Dispose();

            _tcpClient.Dispose();

            GC.SuppressFinalize(this);
        }

        public async Task<bool> AuthorizeClientSession()
        {
            if (string.IsNullOrEmpty(ClientAddress))
            {
                await DataTransmitter.TcpSend(_tcpClient, "Client's endpoint is null or invalid.");
                Dispose();
                return false;
            }

            if (!_connectionLimiter.IsConnectionAllowed(ClientAddress))
            {
                // await DataTransmitter.TcpSend(_tcpClient, "Connection is denied due to max connections.");
                Dispose();
                return false;
            }

            return true;
        }

        public void UpdateLastActivityTime() => _activityTimer.Restart();

        public bool IsSessionTimedOut() => _activityTimer.Elapsed > _sessionTimeout;
    }
}