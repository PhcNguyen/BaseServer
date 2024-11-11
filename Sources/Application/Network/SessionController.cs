using NETServer.Application.Handlers;
using NETServer.Application.Infrastructure;
using NETServer.Application.Network;
using NETServer.Application.Security;
using NETServer.Logging;
using System.Collections.Concurrent;
using System.Net.Sockets;

internal class SessionController
{
    private readonly RequestLimiter _requestLimiter;
    private readonly CommandHandler _commandHandler;
    private readonly ConnectionLimiter _connectionLimiter;

    public readonly ConcurrentDictionary<Guid, ClientSession> ActiveSessions = new();
    public readonly Dictionary<string, DateTime> ClientLastLogTimes = new();

    public SessionController()
    {
        _connectionLimiter = new ConnectionLimiter(Setting.MaxConnectionsPerIp);
        _requestLimiter = new RequestLimiter(Setting.RequestLimit, Setting.LockoutDuration);
        _commandHandler = new CommandHandler();
    }

    public async Task HandleClient(TcpClient client, CancellationToken cancellationToken)
    {
        var session = new ClientSession(client, _requestLimiter, _connectionLimiter, ClientLastLogTimes);

        if (!await session.AuthorizeClientSession())
            return;

        await session.Connect();
        ActiveSessions[session.Id] = session; // Directly store the session

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

                // Update last activity time each time a command is received
                session.UpdateLastActivityTime();

                payload = await _commandHandler.HandleCommand(receivedCommand, receivedData);
            }
        }
        catch (IOException ioEx)
        {
            NLog.Error($"I/O error in client session {session.Id}: {ioEx.Message}");
        }
        catch (SocketException sockEx)
        {
            NLog.Error($"Socket error in client session {session.Id}: {sockEx.Message}");
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
            ActiveSessions.TryRemove(session.Id, out _);
        }
        catch (Exception e)
        {
            NLog.Error($"Error while closing connection for session {session.Id}: {e}");
        }
    }

    private async Task CleanUp()
    {
        var expiredSessions = ActiveSessions
            .Where(kvp => kvp.Value.IsSessionTimedOut())
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var sessionId in expiredSessions)
        {
            if (ActiveSessions.TryRemove(sessionId, out var session))
            {
                await session.Disconnect();
                NLog.Info($"Session {sessionId} removed due to timeout.");
            }
        }

        // Clean expired sessions from ClientLastLogTimes
        var expiredIps = ClientLastLogTimes
            .Where(pair => (DateTime.UtcNow - pair.Value) > Setting.SessionTimeout)
            .Select(pair => pair.Key)
            .ToList();

        foreach (var ip in expiredIps)
        {
            ClientLastLogTimes.Remove(ip);
            NLog.Info($"Session for {ip} removed due to timeout.");
        }
    }

    public async Task RunCleanUp(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await CleanUp();

            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        }

        NLog.Info("The cleanup task has stopped due to a cancellation request.");
    }

    public async Task CloseAllConnections()
    {
        if (ActiveSessions.IsEmpty) return;

        var closeTasks = ActiveSessions.Values
            .Where(session => session.IsConnected)
            .Select(session => CloseConnection(session));

        await Task.WhenAll(closeTasks);

        NLog.Info("All connections closed successfully.");
    }
}
