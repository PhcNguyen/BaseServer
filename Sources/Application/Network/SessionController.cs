using NETServer.Application.Handlers;
using NETServer.Infrastructure.Logging;
using NETServer.Infrastructure.Security;
using NETServer.Infrastructure.Interfaces;
using NETServer.Infrastructure.Configuration;
using NETServer.Application.Network.Transport;

using System.Net.Sockets;
using System.Collections.Concurrent;

namespace NETServer.Application.Network
{
    internal class SessionController : ISessionController
    {
        private readonly ByteBuffer _byteBuffer;
        private readonly SslSecurity _sslSecurity;
        private readonly RequestLimiter _requestLimiter;
        private readonly CommandProcessor _commandHandler;
        private readonly ConnectionLimiter _connectionLimiter;

        
        private readonly ConcurrentDictionary<Guid, ClientSession> _activeSessions = new();
        public IReadOnlyDictionary<Guid, ClientSession> ActiveSessions => _activeSessions;

        public SessionController()
        {
            _byteBuffer = new ByteBuffer();
            _sslSecurity = new SslSecurity();
            _commandHandler = new CommandProcessor();
            _connectionLimiter = new ConnectionLimiter(Setting.MaxConnections);
            _requestLimiter = new RequestLimiter(Setting.RateLimit, Setting.ConnectionLockoutDuration);
        }

        public async Task HandleClient(TcpClient client, CancellationToken cancellationToken)
        {
            ClientSession session = new(client, _requestLimiter, 
                _connectionLimiter, _sslSecurity, _byteBuffer);

            if (!await session.AuthorizeClientSession())
                return;

            await session.Connect().ConfigureAwait(false);
            if (!_activeSessions.TryAdd(session.Id, session))
            {
                NLog.Info($"Session {session.Id} already exists, updating.");
                _activeSessions[session.Id] = session;
            }

            Packet? packet;

            try
            {
                if (session.Transport == null)
                {
                    throw new InvalidOperationException("DataTransport is null. The session is not properly initialized.");
                }

                while (session.IsConnected && !cancellationToken.IsCancellationRequested)
                {
                    // Kiểm tra timeout session
                    if (session.IsSessionTimedOut()) break;

                    // Nhận dữ liệu từ client
                    packet = await session.Transport.ReceiveAsync(cancellationToken).ConfigureAwait(false);

                    // Kiểm tra dữ liệu rỗng
                    if (packet?.Command is null)
                    {
                        await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    // Update last activity time each time a command is received
                    session.UpdateLastActivityTime();

                    await _commandHandler.HandleCommand(session, packet, cancellationToken).ConfigureAwait(false);
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

        public async Task CloseAllConnections()
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
