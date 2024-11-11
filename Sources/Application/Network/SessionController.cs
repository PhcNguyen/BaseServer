using NETServer.Application.Handlers;
using NETServer.Application.Network;
using NETServer.Logging;

using System.Net.Sockets;
using System.Collections.Concurrent;
using NETServer.Infrastructure.Security;
using NETServer.Infrastructure.Configuration;

internal class SessionController
{
    private readonly RequestLimiter _requestLimiter;
    private readonly CommandHandler _commandHandler;
    private readonly ConnectionLimiter _connectionLimiter;

    public readonly ConcurrentDictionary<Guid, WeakReference<ClientSession>> ActiveSessions = new();
    

    public SessionController()
    {
        _connectionLimiter = new ConnectionLimiter(Setting.MaxConnections);
        _requestLimiter = new RequestLimiter(Setting.RateLimit, Setting.ConnectionLockoutDuration);
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
            byte[] receivedData;
            byte[] keyAes = dataTransmitter.KeyAes;

            while (session.IsConnected && !cancellationToken.IsCancellationRequested)
            {
                (receivedCommand, receivedData) = await dataTransmitter.Receive();

                if (receivedCommand == default)
                {
                    if (session.IsSessionTimedOut()) break;

                    await Task.Delay(50);
                    continue;
                }

                // Update last activity time each time a command is received
                session.UpdateLastActivityTime();

                await this.HandleCommand(dataTransmitter, receivedCommand, receivedData);
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

    private async Task HandleCommand(DataTransmitter dataTransmitter, Command command, byte[] data)
    {
        // Xử lý command
        var responseData = await _commandHandler.HandleCommand(command, data);

        // Gửi lại dữ liệu cho client tương ứng (sử dụng session ID)
        await dataTransmitter.Send(responseData);
    }

    private async Task CloseConnection(ClientSession? session)
    {
        if (session == null || !session.IsConnected) return;

        try
        {
            await session.Disconnect();
            // Dọn dẹp WeakReference khi kết thúc phiên
            ActiveSessions.TryRemove(session.Id, out _);
        }
        catch (Exception e)
        {
            NLog.Error($"Error while closing connection for session {session.Id}: {e}");
        }
    }

    public async Task CloseAllConnections()
    {
        if (ActiveSessions.IsEmpty) return;

        var closeTasks = ActiveSessions.Values
            .Select(sessionRef => sessionRef.TryGetTarget(out var session) && session?.IsConnected == true
                ? CloseConnection(session)
                : Task.CompletedTask);

        await Task.WhenAll(closeTasks);

        NLog.Info("All connections closed successfully.");
    }

    public void CleanUpInactiveSessions()
    {
        // Lọc ra các session không còn kết nối và xóa khỏi ActiveSessions
        var inactiveSessions = ActiveSessions
            .Where(sessionRef => !sessionRef.Value.TryGetTarget(out var session) || session == null || !session.IsConnected)
            .ToList(); 

        foreach (var session in inactiveSessions)
        {
            ActiveSessions.TryRemove(session.Key, out _); // Xóa session khỏi ActiveSessions
        }
    }
}