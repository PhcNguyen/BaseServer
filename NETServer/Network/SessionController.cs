using NETServer.Network.Packets;
using NETServer.Network.Firewall;
using NETServer.Network.Security;
using NETServer.Application.Handlers;
using NETServer.Infrastructure.Logging;
using NETServer.Infrastructure.Interfaces;
using NETServer.Infrastructure.Configuration;

using System.Net.Sockets;
using System.Collections.Concurrent;
using NETServer.Infrastructure.Helper;

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

        public SessionController()
        {
            _byteBuffer = new ByteBuffer();
            _sslSecurity = new SslSecurity();
            _commandHandler = new CommandHandler();
            _connectionLimiter = new ConnLimiter(Setting.MaxConnections);
            _requestLimiter = new RequestLimiter(Setting.RateLimit, Setting.ConnectionLockoutDuration);
        }

        public async Task HandleClientAsync(TcpClient tcpClient, CancellationToken cancellationToken)
        {
            ClientSession session = new(tcpClient, _byteBuffer, _sslSecurity, _connectionLimiter);

            if (!await session.AuthorizeClientSession())
                return;

            await session.Connect().ConfigureAwait(false);
            if (!_activeSessions.TryAdd(session.Id, session))
            {
                NLog.Info($"Session {session.Id} already exists, updating.");
                _activeSessions[session.Id] = session;
            }
            PacketContainer packetContainer = new();

            try
            {
                if (session.Transport == null)
                {
                    throw new InvalidOperationException("DataTransport is null. The session is not properly initialized.");
                }

                while (session.IsConnected && !cancellationToken.IsCancellationRequested)
                {
                    if (session.IsSessionTimedOut()) break;

                    // Nhận gói tin từ client
                    if (await session.Transport.ReceiveAsync(cancellationToken).ConfigureAwait(false) is not Packet packet)
                    {
                        // Đợi một chút trước khi nhận gói tin tiếp theo
                        await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    if (packet?.Command == null)
                    {
                        // Đợi một chút trước khi nhận gói tin tiếp theo
                        await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    // Thêm gói tin vào container
                    packetContainer.AddPacket(packet);

                    await ProcessPacketAsync(session, packetContainer, cancellationToken).ConfigureAwait(false);

                    // Đợi một chút trước khi nhận gói tin tiếp theo
                    await Task.Delay(50, cancellationToken).ConfigureAwait(false);  
                }
            }
            catch (IOException ioEx)
            {
                NLog.Error($"I/O error in client session {session.ClientAddress}: {ioEx.Message} - StackTrace: {ioEx.StackTrace}");
            }
            catch (SocketException sockEx)
            {
                NLog.Error($"Socket error in client session {session.ClientAddress}: {sockEx.Message}");
            }
            catch (Exception ex)
            {
                NLog.Error($"General error in client session {session.ClientAddress}: {ex.Message}");
            }
            finally
            {
                await CloseConnection(session).ConfigureAwait(false);
            }
        }

        private async Task ProcessPacketAsync(ClientSession session, PacketContainer packetContainer, CancellationToken cancellationToken)
        {
            // Đợi khi có không gian trong semaphore để xử lý gói tin
            await session.TaskSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            if (!(session.TaskSemaphore.CurrentCount > 0))
            {
                // Đợi một chút trước khi nhận gói tin tiếp theo
                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                return;
            }

            // Xử lý gói tin trong một task riêng biệt để không làm gián đoạn việc nhận gói tin mới
            _ = Task.Run(async () =>
            {
                try
                {
                    // Lấy gói tin từ container và xử lý
                    if (packetContainer.DequeuePacket(out var packetToProcess) && packetToProcess != null)
                    {
                        await _commandHandler.HandleCommand(session, packetToProcess, cancellationToken).ConfigureAwait(false);

                        // Cập nhật thời gian hoạt động của session mỗi khi có gói tin mới
                        session.UpdateLastActivityTime();
                        _requestLimiter.IsAllowed(session.ClientAddress);
                    }
                }
                finally
                {
                    // Giải phóng semaphore khi hoàn thành xử lý
                    session.TaskSemaphore.Release(); 
                }
            }, cancellationToken);
        }

        private async Task CloseConnection(ClientSession session)
        {
            if (session == null || !session.IsConnected) return;

            try
            {
                await session.Disconnect().ConfigureAwait(false);
                _activeSessions.TryRemove(session.Id, out _); // Xóa session khỏi ActiveSessions
            }
            catch (Exception e)
            {
                NLog.Error($"Error while closing connection for session {session.Id}: {e}");
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

        public void CleanUpInactiveSessions()
        {
            var inactiveSessions = _activeSessions
                .Where(session => !session.Value.IsConnected || session.Value.IsSessionTimedOut())
                .ToList();

            foreach (var session in inactiveSessions)
            {
                _activeSessions.TryRemove(session.Key, out _); // Xóa session khỏi ActiveSessions
            }

            NLog.Info("Inactive sessions cleaned up.");
        }

        public async Task CleanUpInactiveSessionsPeriodically(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);  // Đợi 5 phút hoặc hủy

                if (cancellationToken.IsCancellationRequested)
                    break;

                var inactiveSessions = _activeSessions
                    .Where(session => !session.Value.IsConnected || session.Value.IsSessionTimedOut())
                    .ToList();

                foreach (var session in inactiveSessions)
                {
                    _activeSessions.TryRemove(session.Key, out _);
                }

                NLog.Info("Inactive sessions cleaned up periodically.");
            }
        }
    }
}
