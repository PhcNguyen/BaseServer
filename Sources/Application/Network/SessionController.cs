using System.Net.Sockets;
using System.Collections.Concurrent;
using NETServer.Logging;
using NETServer.Infrastructure;
using NETServer.Application.Security;

namespace NETServer.Application.NetSocketServer;

internal class SessionController
{
    private readonly RequestLimiter _requestLimiter;
    private readonly ConnectionLimiter _connectionLimiter;

    private readonly ConcurrentDictionary<Guid, WeakReference<ClientSession>> _activeSessions = new();

    public SessionController()
    {
        _connectionLimiter = new ConnectionLimiter(Setting.MaxConnectionsPerIp);
        _requestLimiter = new RequestLimiter(Setting.Limit, Setting.TimeWindow, Setting.LockoutDuration);
    }

    public async Task HandleClient(TcpClient client, CancellationToken cancellationToken)
    {
        var session = new ClientSession(client, _requestLimiter, _connectionLimiter);

        if (!await session.AuthorizeClientSession())
            return;

        await session.Connect();
        _activeSessions[session.Id] = new WeakReference<ClientSession>(session);

        try
        {
            while (session.IsConnected && !cancellationToken.IsCancellationRequested)
            {
                // await session.ReceiveDataAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            NLog.Error($"Error in client session {session.Id}: {ex}");
        }
        finally
        {
            await CloseConnection(session);
        }
    }

    private async Task CloseConnection(ClientSession session)
    {
        if (!session.IsConnected) return;

        try
        {
            await session.Disconnect();

            _activeSessions.TryRemove(session.Id, out var _);
            NLog.Info($"Session {session.Id} from {session.ClientAddress} disconnected.");
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
            .Select(weakRef => weakRef.TryGetTarget(out var session) ? CloseConnection(session) : Task.CompletedTask)
            .ToList();

        await Task.WhenAll(closeTasks);
        NLog.Info("All connections closed successfully.");
    }
}