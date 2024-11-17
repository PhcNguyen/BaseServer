using NETServer.Infrastructure.Configuration;
using NETServer.Infrastructure.Logging;
using System.Net.Sockets;
using System.Net;

namespace NETServer.Network
{
    internal class ServerEngine
    {
        private int _isRunning;
        private bool _isInMaintenanceMode = false;
        private readonly int _maxConnections = Setting.MaxConnections;
        private readonly TcpListener _tcpListener;
        private readonly SessionController _sessionController;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public ServerEngine()
        {
            _isRunning = 0;
            _sessionController = new SessionController();
            _tcpListener = new TcpListener(
                Setting.IPAddress == null ? IPAddress.Any : IPAddress.Parse(Setting.IPAddress),
                Setting.Port
            );

            ConfigureSocket(_tcpListener.Server);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private static void ConfigureSocket(Socket socket)
        {
            socket.Blocking = Setting.Blocking;
            socket.Listen(Setting.QueueSize);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, Setting.KeepAlive);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, Setting.SendTimeout);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, Setting.ReuseAddress);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, Setting.ReceiveTimeout);
        }

        private async ValueTask AcceptClientConnectionsAsync(CancellationToken token)
        {
            while (_isRunning == 1 && !token.IsCancellationRequested)
            {
                if (_isInMaintenanceMode)
                {
                    NLog.Warning("Server in maintenance mode.");
                    await Task.Delay(5000, token);
                    continue;
                }

                if (_sessionController.ActiveSessions.Count >= _maxConnections)
                {
                    NLog.Warning("Maximum connections reached.");
                    await Task.Delay(1000, token);  // Delay ngắn để tránh liên tục kiểm tra
                    continue;
                }

                try
                {
                    var client = await _tcpListener.AcceptTcpClientAsync(token);
                    _ = _sessionController.HandleClientAsync(client, token);
                }
                catch (Exception ex) when (token.IsCancellationRequested)
                {
                    NLog.Error($"Server stopped accepting clients due to cancellation - Exception: {ex}");
                    break;
                }
                catch (Exception ex)
                {
                    NLog.Error(ex);
                }
            }
        }

        public void StartServer()
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
            {
                NLog.Warning("Server is already running.");
                return;
            }

            var token = _cancellationTokenSource.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    _tcpListener.Start();
                    NLog.Info($"Server started and listening on {_tcpListener.LocalEndpoint}");

                    await AcceptClientConnectionsAsync(token);
                }
                catch (Exception ex)
                {
                    NLog.Error(ex);
                }
            }, token);
        }

        public void SetMaintenanceMode(bool isMaintenance)
        {
            _isInMaintenanceMode = isMaintenance;
            if (isMaintenance)
            {
                NLog.Info("Server is now in maintenance mode.");
            }
            else
            {
                NLog.Info("Server has exited maintenance mode.");
            }
        }

        public void StopServer()
        {
            if (Interlocked.CompareExchange(ref _isRunning, 0, 1) == 0)
            {
                NLog.Warning("Server is not running.");
                return;
            }

            _cancellationTokenSource.Cancel();
            Task.Run(async () => await _sessionController.CloseAllConnections());
            _tcpListener.Stop();
            NLog.Info("Server has stopped successfully.");
        }

        public void ResetServer()
        {
            StopServer();
            StartServer();
            NLog.Info("Server has been reset successfully.");
        }
    }
}
