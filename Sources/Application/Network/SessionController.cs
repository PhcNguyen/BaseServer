using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;

using NETServer.Logging;
using NETServer.Infrastructure;
using NETServer.Application.Security;

namespace NETServer.Application.Network;

internal class SessionController
{
    private readonly RequestLimiter _requestLimiter;
    private readonly ConnectionLimiter _connectionLimiter;

    private readonly ConcurrentBag<ClientSession> _activeSessions = new ConcurrentBag<ClientSession>();
    private readonly ConcurrentDictionary<string, int> _ipConnectionCounts = new ConcurrentDictionary<string, int>();

    public SessionController()
    {
        _connectionLimiter = new ConnectionLimiter( Setting.MaxConnectionsPerIp );
        _requestLimiter = new RequestLimiter(Setting.Limit, Setting.TimeWindow, Setting.LockoutDuration);
    }

    private async Task HandleClient(TcpClient client)
    {
        ClientSession session = new ClientSession(client);
        await session.Connect();

        if (!await AuthorizeClientSession(session)) return;

        // Thêm session vào danh sách active
        _activeSessions.Add(session);

        try
        {
            // Trong khi client còn kết nối
            while (session.IsConnected)
            {
                // Logic xử lý client tại đây
                // Ví dụ, bạn có thể nhận dữ liệu từ client và xử lý.
                await Task.Delay(100); // Giảm tải CPU, tránh vòng lặp không cần thiết.
                await session.Disconnect();
            }
        }
        catch (Exception ex)
        {
            NLog.Error(ex);
        }
        finally
        {
            // Đảm bảo kết nối luôn được đóng
            await CloseConnection(session);
        }
    }

    public async Task AcceptClientConnection(TcpClient client)
    {
        try
        {
            if (await HandleClientConnectionAcceptance(client))
            {
                _ = Task.Run(() => HandleClient(client));  // Xử lý client nếu kết nối hợp lệ
            }
            else
            {
                client.Close();  // Đóng kết nối nếu không hợp lệ
            }
        }
        catch (Exception ex)
        {
            NLog.Error(ex);
            client.Close();
        }
    }

    private async Task<bool> HandleClientConnectionAcceptance(TcpClient client)
    {
        if (client.Client.RemoteEndPoint is IPEndPoint clientEndPoint)
        {
            string clientIp = clientEndPoint.Address.ToString();
            if (await _requestLimiter.IsAllowed(clientIp))
            {
                if (client?.Client.Connected ?? false)
                {
                    return true;
                }
                else
                {
                    NLog.Warning($"Client {clientIp} is not connected.");
                }
            }
            else
            {
                NLog.Warning($"Connection from {clientIp} is not allowed.");
            }
        }
        else
        {
            NLog.Warning("Client's endpoint is null or invalid.");
        }

        return false;
    }

    private async Task CloseConnection(ClientSession session)
    {
        if (!session.IsConnected) return;

        try
        {
            // Ngắt kết nối client
            await session.Disconnect();

            // Xóa session khỏi danh sách, sử dụng TryTake cho ConcurrentBag
            if (_activeSessions.TryTake(out var removedSession))
            {
                NLog.Info($"Session for {removedSession.TcpClient.Client.RemoteEndPoint} removed.");
            }

            if (session.TcpClient.Client.RemoteEndPoint is IPEndPoint clientEndPoint)
            {
                string clientIp = clientEndPoint.Address.ToString();
                _ipConnectionCounts.AddOrUpdate(clientIp, 0, (key, oldValue) => Math.Max(0, oldValue - 1));
            }
        }
        catch (Exception e)
        {
            NLog.Error($"Error while closing connection: {e}");
        }
    }

    public async Task CloseAllConnections()
    {
        if (_activeSessions.IsEmpty) return;

        var closeTasks = _activeSessions.Select(session => CloseConnection(session)).ToList();
        await Task.WhenAll(closeTasks);  // Đảm bảo các kết nối được đóng đồng thời

        // Sau khi đóng tất cả, xóa các session
        _activeSessions.Clear();
        NLog.Info("All connections closed successfully.");
    }

    private async Task<bool> AuthorizeClientSession(ClientSession session)
    {
        if (session.TcpClient.Client.RemoteEndPoint is IPEndPoint clientEndPoint)
        {
            string clientIp = clientEndPoint.Address.ToString();

            if (!_connectionLimiter.IsConnectionAllowed(clientIp))
            {
                NLog.Warning($"Connection from {clientIp} is denied due to max connections.");
                await session.Disconnect();
                return false;
            }

            if (!await _requestLimiter.IsAllowed(clientIp))
            {
                NLog.Warning($"Request from {clientIp} is denied due to rate limit.");
                await session.Disconnect();
                return false;
            }

            return true;
        }

        NLog.Warning("Client's endpoint is null or invalid.");
        await session.Disconnect();
        return false;
    }
}