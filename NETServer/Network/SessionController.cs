using NETServer.Application.Handlers;
using NETServer.Infrastructure.Configuration;
using NETServer.Infrastructure.Helper;
using NETServer.Infrastructure.Interfaces;
using NETServer.Infrastructure.Logging;
using NETServer.Network.Firewall;
using NETServer.Network.Packets;
using NETServer.Network.Security;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Channels;

namespace NETServer.Network
{
    internal class SessionController : ISessionController
    {
        private readonly ByteBuffer _byteBuffer;
        private readonly SslSecurity _sslSecurity;
        private readonly RequestLimiter _requestLimiter;
        private readonly CommandHandler _commandHandler;
        private readonly ConnLimiter _connectionLimiter;

        private readonly ConcurrentDictionary<Guid, ClientSession> _activeSessions = new();
        public IReadOnlyDictionary<Guid, ClientSession> ActiveSessions => _activeSessions;

        // Kênh dùng để trao đổi gói tin giữa các task nhận và xử lý
        private readonly Channel<Packet> _packetChannel = Channel.CreateUnbounded<Packet>();

        public SessionController()
        {
            _byteBuffer = new ByteBuffer();
            _sslSecurity = new SslSecurity();
            _commandHandler = new CommandHandler();
            _connectionLimiter = new ConnLimiter(Setting.MaxConnections);
            _requestLimiter = new RequestLimiter(Setting.RateLimit, Setting.ConnectionLockoutDuration);
        }

        public async Task StartProcessingSessions(CancellationToken cancellationToken)
        {
            var receiveTask = Task.Run(() => ReceivePacketsAsync(cancellationToken), cancellationToken);
            var processTask = Task.Run(() => ProcessPacketsAsync(cancellationToken), cancellationToken);

            await Task.WhenAll(receiveTask, processTask).ConfigureAwait(false);
        }

        public async Task AcceptClientAsync(TcpClient tcpClient)
        {
            ClientSession session = new(tcpClient, _byteBuffer, _sslSecurity, _connectionLimiter);

            if (!await session.AuthorizeClientSession())
                return;

            await session.Connect();
            if (!_activeSessions.TryAdd(session.ID, session))
            {
                NLog.Info($"Session {session.ID} already exists, updating.");
                _activeSessions[session.ID] = session;
            }
        }

        public async ValueTask CloseAllConnections()
        {
            if (_activeSessions.IsEmpty) return;

            var closeTasks = _activeSessions.Values
                .Where(session => session.IsConnected)
                .Select(session => CloseConnection(session))
                .ToList(); // Chuyển sang List để kiểm soát số lượng task đồng thời

            while (closeTasks.Count != 0)
            {
                var batch = closeTasks.Take(10).ToList(); // Giới hạn tối đa 10 kết nối cùng lúc
                closeTasks = closeTasks.Skip(10).ToList();

                await Task.WhenAll(batch).ConfigureAwait(false);
            }

            NLog.Info("All connections closed successfully.");
        }

        private async Task ReceivePacketsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var session in _activeSessions.Values)
                {
                    if (session.IsSessionTimedOut() || session.Transport == null)
                    {
                        await CloseConnection(session);
                        continue;
                    }

                    // Nhận gói tin từ client
                    if (await session.Transport.ReceiveAsync(cancellationToken).ConfigureAwait(false) is not Packet packet)
                        continue;

                    if (packet?.Command == null)
                        continue;

                    // Đẩy gói tin vào kênh
                    await _packetChannel.Writer.WriteAsync(packet, cancellationToken).ConfigureAwait(false);
                }

                // Đợi một chút trước khi kiểm tra lại các session
                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task ProcessPacketsAsync(CancellationToken cancellationToken)
        {
            await foreach (var packet in _packetChannel.Reader.ReadAllAsync(cancellationToken))
            {
                if (packet?.Command == null) continue;

                var session = _activeSessions.Values.FirstOrDefault(s => s.ID == packet.ID);

                if (session == null) continue;

                try
                {
                    // Xử lý gói tin
                    await _commandHandler.HandleCommand(session, packet, cancellationToken).ConfigureAwait(false);

                    // Cập nhật thời gian hoạt động của session
                    session.UpdateLastActivityTime();
                    _requestLimiter.IsAllowed(session.ClientAddress);
                }
                catch (Exception ex)
                {
                    NLog.Error($"Error processing packet for session {session.ClientAddress}: {ex.Message}");
                }
            }
        }

        private async Task CloseConnection(ClientSession session)
        {
            if (session == null || !session.IsConnected) return;

            try
            {
                await session.Disconnect();
                _activeSessions.TryRemove(session.ID, out _); // Xóa session khỏi ActiveSessions
            }
            catch (Exception e)
            {
                NLog.Error($"Error while closing connection for session {session.ID}: {e}");
            }
        }

    }
}