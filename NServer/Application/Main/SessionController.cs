using NServer.Infrastructure.Services;
using NServer.Interfaces.Core.Network;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using NServer.Application.Handler;
using NServer.Core.Network;
using NServer.Core.Network.Buffers;
using NServer.Core.Logging;
using NServer.Core.Packet;
using NServer.Core.Packet.Utils;

namespace NServer.Application.Main
{
    internal class SessionController
    {
        private readonly ConcurrentDictionary<Guid, ISession> _activeSessions = new();
        private readonly CommandHandler _commandHandler = Singleton.GetInstance<CommandHandler>();
        private readonly MultiSizeBuffer _multiSizeBuffer = Singleton.GetInstance<MultiSizeBuffer>();
        private readonly PacketContainer _packetContainer = Singleton.GetInstance<PacketContainer>();
        private readonly CancellationToken _cancellationToken;

        public IReadOnlyDictionary<Guid, ISession> ActiveSessions => _activeSessions;
       
        public SessionController(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _multiSizeBuffer.AllocateBuffers();

            _ = Task.Run(async () => await this.MonitorSessionsAsync(_cancellationToken), cancellationToken);
            _ = Task.Run(async () => await this.ProcessPacketsAsync(_cancellationToken), cancellationToken);
        }

        public async Task AcceptClientAsync(Socket clientSocket)
        {
            Session session = new(clientSocket);

            if (!_activeSessions.TryAdd(session.Id, session))
            {
                _activeSessions[session.Id] = session;
            }

            await session.Connect();
        }

        public async Task ProcessPacketsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var tasks = _packetContainer.UsersChannels
                    .Select(kvp => ProcessChannelPacketsAsync(kvp.Key, kvp.Value, cancellationToken))
                    .ToList();

                await Task.WhenAll(tasks);

                await Task.Delay(100, cancellationToken); // Điều chỉnh độ trễ theo nhu cầu
            }
        }

        private async Task ProcessChannelPacketsAsync(Guid userId, Channel<Packets> channel, CancellationToken cancellationToken)
        {
            await foreach (var packet in channel.Reader.ReadAllAsync(cancellationToken))
            {
                if (packet?.Command == null) continue;

                if (!_activeSessions.TryGetValue(userId, out var session)) continue;

                try
                {
                    await _commandHandler.HandleCommand(session, packet, cancellationToken).ConfigureAwait(false);

                    session.UpdateLastActivityTime();
                }
                catch (Exception ex)
                {
                    NLog.Error($"Error processing packet for session {session?.Ip}: {ex.Message}");
                }
            }
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
