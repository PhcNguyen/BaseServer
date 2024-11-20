using NETServer.Application.Handlers;
using NETServer.Core.Network;
using NETServer.Core.Network.Buffers;
using NETServer.Core.Network.Packet;
using NETServer.Infrastructure.Logging;
using NETServer.Infrastructure.Services;
using NETServer.Interfaces.Core.Network;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace NETServer.Application.Main
{
    internal class SessionController
    {
        private readonly ConcurrentDictionary<Guid, ISession> _activeSessions = new();
        private readonly CommandHandler _commandHandler = Singleton.GetInstance<CommandHandler>();
        private readonly MultiSizeBuffer _multiSizeBuffer = Singleton.GetInstance<MultiSizeBuffer>();
        private readonly PacketContainer _packetContainer = Singleton.GetInstance<PacketContainer>();

        public IReadOnlyDictionary<Guid, ISession> ActiveSessions => _activeSessions;

        public SessionController()
        {
            _multiSizeBuffer.AllocateBuffers();
        }

        public void AcceptClient(Socket clientSocket)
        {
            Session session = new(clientSocket);

            if (!_activeSessions.TryAdd(session.Id, session))
            {
                NLog.Info($"Session {session.Id} already exists, updating.");
                _activeSessions[session.Id] = session;
            }
        }

        public async Task ProcessPacketsAsync(CancellationToken cancellationToken)
        {
            var tasks = _packetContainer.UsersChannels.Values.Select(async channel =>
            {
                await foreach (var packet in channel.Reader.ReadAllAsync(cancellationToken))
                {
                    if (packet?.Command == null) continue;

                    var sessionId = packet.Id;
                    if (!_activeSessions.TryGetValue(sessionId, out var session)) continue;

                    try
                    {
                        await _commandHandler.HandleCommand(session, packet, cancellationToken).ConfigureAwait(false);

                        // Cập nhật thời gian hoạt động của session
                        //_requestLimiter.IsAllowed(socketHandler.Ip);
                        session.UpdateLastActivityTime();
                    }
                    catch (Exception ex)
                    {
                        NLog.Error($"Error processing packet for session {session.Ip}: {ex.Message}");
                    }
                }
            }).ToList();

            await Task.WhenAll(tasks);
        }

        public async Task MonitorSessionsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var session in _activeSessions.Values)
                {
                    if (session.IsSessionTimedOut())
                    {
                        await CloseConnection(session);
                        continue;
                    }
                }

                await Task.Delay(2000, cancellationToken).ConfigureAwait(false);
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

        private async Task CloseConnection(ISession session)
        {
            if (session == null) return;

            try
            {
                _activeSessions.TryRemove(session.Id, out _);
                await session.Disconnect();
            }
            catch (Exception e)
            {
                NLog.Error($"Error while closing connection for session {session.Id}: {e}");
            }
        }
    }
}
