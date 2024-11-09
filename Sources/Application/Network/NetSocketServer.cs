using NETServer.Application.NetSocketServer;
using NETServer.Infrastructure;
using NETServer.Logging;
using System.Net.Sockets;
using System.Net;

namespace NETServer.Application.NetSocketServer;

internal class NetSocketServer
{
    private bool _isRunning;
    private readonly TcpListener _tcpListener;
    private readonly SessionController _sessionController;
    private CancellationTokenSource _cancellationTokenSource;

    public NetSocketServer()
    {
        _isRunning = false;
        _sessionController = new SessionController();
        _tcpListener = new TcpListener(IPAddress.Any, Setting.Port);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void StartListener()
    {
        if (_isRunning)
        {
            NLog.Warning("Server is already running");
            return;
        }

        // Khởi động server bất đồng bộ với CancellationToken để dễ dàng dừng
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                _tcpListener.Start();
                _isRunning = true;
                NLog.Info($"Server started and listening on {_tcpListener.LocalEndpoint}");

                while (_isRunning && !token.IsCancellationRequested)
                {
                    try
                    {
                        TcpClient client = await _tcpListener.AcceptTcpClientAsync();
                        _ = Task.Run(() => _sessionController.HandleClient(client, token), token);
                    }
                    catch (SocketException ex) when (token.IsCancellationRequested)
                    {
                        NLog.Error(ex, "Listener stopped due to cancellation.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        NLog.Error(ex, "Unexpected error in listener loop");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                NLog.Error(ex, $"Error during listener task");
            }
            finally
            {
                _isRunning = false;
                NLog.Info("Listener task has ended.");
            }
        }, token);
    }

    public void StopListener()
    {
        if (!_isRunning)
        {
            NLog.Warning("Server is not running.");
            return;
        }

        // Dừng listener và huỷ các phiên đang hoạt động
        _isRunning = false;
        _cancellationTokenSource.Cancel();

        Task.Run(() =>
        {
            try
            {
                // Wait for all connections to close
                _sessionController.CloseAllConnections().Wait();  
                NLog.Info("All client sessions have been closed.");
            }
            catch (Exception ex)
            {
                NLog.Error(ex, "Error while closing client sessions");
            }
        });

        _tcpListener.Stop();
        NLog.Info("Server has stopped successfully.");
    }
}
