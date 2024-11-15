using NETServer.Infrastructure.Configuration;
using NETServer.Infrastructure.Logging;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;

namespace NETServer.Application.Network
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
            _isRunning = 0; // 0: false, 1: true
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

        private async Task AcceptClientConnectionsAsync(CancellationToken token)
        {
            while (_isRunning == 1 && !token.IsCancellationRequested)
            {
                if (_isInMaintenanceMode)
                {
                    NLog.Warning("Server in maintenance mode.");
                    await Task.Delay(5000, token);
                    continue;
                }

                // Kiểm tra ngay session
                if (_sessionController.ActiveSessions.Count >= _maxConnections)
                {
                    NLog.Warning("Maximum connections reached.");
                    continue;
                }

                try
                {
                    var client = await _tcpListener.AcceptTcpClientAsync(token);
                    _ = _sessionController.HandleClient(client, token);
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

            // Chạy server trong một Task riêng
            _ = Task.Run(async () =>
            {
                try
                {
                    _tcpListener.Start();
                    NLog.Info($"Server started and listening on {_tcpListener.LocalEndpoint}");

                    // Tiến hành nhận kết nối trong khi server đang chạy và chưa bị hủy
                    await this.AcceptClientConnectionsAsync(token);
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
            StopServer();  // Dừng server trước
            StartServer(); // Khởi động lại server
            NLog.Info("Server has been reset successfully.");
        }
    }
}
