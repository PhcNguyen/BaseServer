using NETServer.Logging;
using NETServer.Application.Infrastructure;

using System.Net;
using System.Net.Sockets;

namespace NETServer.Application.Network;

using System.Threading;

internal class Server
{
    private int _isRunning;
    private readonly int MaxConnections = Setting.MaxConnections;
    private readonly TcpListener _tcpListener;
    private readonly SessionController _sessionController;
    private CancellationTokenSource _cancellationTokenSource;

    public Server()
    {
        _isRunning = 0;  // 0:false - 1:true
        _sessionController = new SessionController();
        _tcpListener = new TcpListener(
             Setting.IPAddress == null ? IPAddress.Any : IPAddress.Parse(Setting.IPAddress),
             Setting.Port
         );
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void StartServer()
    {
        if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
        {
            NLog.Warning("Server is already running");
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                _tcpListener.Start();
                NLog.Info($"Server started and listening on {_tcpListener.LocalEndpoint}");

                await _sessionController.RunCleanUp(token);

                while (_isRunning == 1 && !token.IsCancellationRequested)
                {
                    if (_sessionController.ActiveSessions.Count >= MaxConnections)
                    {
                        NLog.Warning("Maximum server connections reached. Refusing new connection.");
                        return;
                    }

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
                NLog.Error(ex, "Error during listener task");
            }
            finally
            {
                Interlocked.Exchange(ref _isRunning, 0);
                NLog.Info("Listener task has ended.");
            }
        }, token);
    }

    public void StopServer()
    {
        if (Interlocked.CompareExchange(ref _isRunning, 0, 1) == 0)
        {
            NLog.Warning("Server is not running.");
            return;
        }

        _cancellationTokenSource.Cancel();

        Task.Run(async () =>
        {
            try
            {
                await _sessionController.CloseAllConnections();
            }
            catch (Exception ex)
            {
                NLog.Error(ex, "Error while closing client sessions");
            }
        });

        _tcpListener.Stop();
        NLog.Info("Server has stopped successfully.");
    }

    public void ResetServer()
    {
        StopServer();  // Dừng server trước
        StartServer(); // Khởi động lại server
        NLog.Info("Server has been reset successfully.");
    }
}