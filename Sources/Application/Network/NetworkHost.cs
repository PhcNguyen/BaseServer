using System.Net;
using System.Net.Sockets;
using NETServer.Logging;
using NETServer.Infrastructure;

namespace NETServer.Application.Network;

internal class NetworkHost
{
    private bool _isRunning;
    private readonly TcpListener _tcpListener;
    private readonly SessionController _sessionController;

    public NetworkHost()
    {
        _isRunning = false;
        _sessionController = new SessionController();
        _tcpListener = new TcpListener(IPAddress.Any, Setting.Port);
    }

    public void StartListener()
    {
        if (_isRunning)
        {
            NLog.Warning("Server is already running");
            return;
        }

        // Start accepting client connections asynchronously in a separate task
        Task.Run(async () =>
        {
            try
            {
                _tcpListener.Start();
                _isRunning = true;

                NLog.Info($"Server started and listening on {_tcpListener.LocalEndpoint}");

                while (_isRunning)
                {
                    try
                    {
                        // Use AcceptTcpClientAsync to avoid blocking the thread
                        TcpClient client = await _tcpListener.AcceptTcpClientAsync();
                        _ = Task.Run(() => _sessionController.AcceptClientConnection(client));
                    }
                    catch (SocketException ex)
                    {
                        NLog.Error($"SocketException: {ex.Message}. Server startup failed.");
                        break;  // Exit if there is a socket error
                    }
                    catch (Exception ex)
                    {
                        NLog.Error($"Unexpected error: {ex.Message}");
                        break;  // Exit on other unexpected errors
                    }
                }
            }
            catch (Exception ex)
            {
                NLog.Error($"Error during listener task: {ex.Message}");
            }
        });
    }

    public void StopListener()
    {
        if (!_isRunning)
        {
            NLog.Warning("Server not running");
            return;
        }

        _isRunning = false;

        // Đảm bảo đóng các kết nối
        Task closeConnectionsTask = _sessionController.CloseAllConnections();
        closeConnectionsTask.Wait();  // This will block until the task is complete

        _tcpListener.Stop();

        NLog.Info("The server has stopped");
    }
}
