using NETServer.Logging;
using NETServer.Infrastructure;
using NETServer.Application.Handlers;

using System.Net.Sockets;
using System.Collections.Concurrent;

namespace NETServer.Application.Network;

internal class SessionController
{
    private readonly RequestLimiter _requestLimiter;
    private readonly CommandHandler _commandHandler;
    private readonly ConnectionLimiter _connectionLimiter;

    public readonly ConcurrentDictionary<Guid, WeakReference<ClientSession>> ActiveSessions = new();

    public SessionController()
    {
        _connectionLimiter = new ConnectionLimiter(Setting.MaxConnectionsPerIp);
        _requestLimiter = new RequestLimiter(Setting.RequestLimit, Setting.LockoutDuration);
        _commandHandler = new CommandHandler();
    }

    public async Task HandleClient(TcpClient client, CancellationToken cancellationToken)
    {
        var session = new ClientSession(client, _requestLimiter, _connectionLimiter);

        if (!await session.AuthorizeClientSession())
            return;

        await session.Connect();
        ActiveSessions[session.Id] = new WeakReference<ClientSession>(session);

        try
        {
            if (session.ClientStream is not Stream clientStream) return;

            var dataTransmitter = new DataTransmitter(clientStream);

            Command receivedCommand;

            byte[] payload;
            byte[] receivedData;
            byte[] keyAes = dataTransmitter.KeyAes;

            while (session.IsConnected && !cancellationToken.IsCancellationRequested)
            {
                (receivedCommand, receivedData) = await dataTransmitter.Receive(cancellationToken);

                if (receivedCommand == default) break;

                payload = await _commandHandler.HandleCommand(receivedCommand, receivedData);
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

            ActiveSessions.TryRemove(session.Id, out var _);
            NLog.Info($"Session {session.Id} from {session.ClientAddress} disconnected.");
        }
        catch (Exception e)
        {
            NLog.Error($"Error while closing connection for session {session.Id}: {e}");
        }
    }

    public async Task CloseAllConnections()
    {
        if (ActiveSessions.IsEmpty) return;
        else
        {
            var closeTasks = ActiveSessions.Values
            .Select(weakRef => weakRef.TryGetTarget(out var session) ? CloseConnection(session) : Task.CompletedTask)
            .ToList();

            await Task.WhenAll(closeTasks);
        }
        
        NLog.Info("All connections closed successfully.");
    }
}