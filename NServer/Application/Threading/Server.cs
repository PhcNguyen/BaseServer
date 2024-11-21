using System.Net.Sockets;
using System.Net;
using NServer.Application.Main;
using NServer.Infrastructure.Configuration;
using NServer.Core.Logging;

namespace NServer.Application.Threading
{
    internal class Server
    {
        private int _isRunning;
        private bool _isInMaintenanceMode = false;
        private readonly int _maxConnections;
        private readonly Socket _listenerSocket;
        private readonly SessionController _sessionController;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public Server()
        {
            _isRunning = 0;
            _cancellationTokenSource = new CancellationTokenSource();
            _maxConnections = Setting.MaxConnections;
            _sessionController = new SessionController(_cancellationTokenSource.Token);
            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            ConfigureSocket(_listenerSocket);
        }

        private static void ConfigureSocket(Socket socket)
        {
            socket.Blocking = Setting.Blocking;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, Setting.KeepAlive);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, Setting.ReuseAddress);
            socket.ReceiveTimeout = Setting.ReceiveTimeout;
            socket.SendTimeout = Setting.SendTimeout;
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
                    var localEndPoint = new IPEndPoint(
                        string.IsNullOrEmpty(Setting.IPAddress) ? IPAddress.Any : IPAddress.Parse(Setting.IPAddress),
                        Setting.Port);

                    _listenerSocket.Bind(localEndPoint);
                    _listenerSocket.Listen(_maxConnections);

                    NLog.Info($"Server started and listening on {localEndPoint}");
                    await AcceptClientConnectionsAsync(token);
                }
                catch (Exception ex)
                {
                    NLog.Error(ex);
                    StopServer();
                }
            }, token);
        }

        private async Task AcceptClientConnectionsAsync(CancellationToken token)
        {
            while (_isRunning == 1)
            {
                if (token.IsCancellationRequested)
                {
                    NLog.Warning("Server stopping due to cancellation request.");
                    break;
                }

                if (_isInMaintenanceMode)
                {
                    NLog.Warning("Server in maintenance mode.");
                    await Task.Delay(5000, token);
                    continue;
                }

                if (_sessionController.ActiveSessions.Count >= _maxConnections)
                {
                    NLog.Warning("Maximum connections reached.");
                    await Task.Delay(1000, token); // Delay để giảm tải vòng lặp
                    continue;
                }

                try
                {
                    var acceptSocket = await Task.Factory.FromAsync(
                        _listenerSocket.BeginAccept,
                        _listenerSocket.EndAccept,
                        null).ConfigureAwait(false);

                    ConfigureSocket(acceptSocket); // Cấu hình socket của client
                    await _sessionController.AcceptClientAsync(acceptSocket);
                }
                catch (SocketException ex)
                {
                    NLog.Error($"Socket error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    NLog.Error($"Unexpected error: {ex.Message}");
                }
            }
        }

        public void SetMaintenanceMode(bool isMaintenance)
        {
            _isInMaintenanceMode = isMaintenance;
            NLog.Info(isMaintenance
                ? "Server is now in maintenance mode."
                : "Server has exited maintenance mode.");
        }

        public void StopServer()
        {
            if (Interlocked.CompareExchange(ref _isRunning, 0, 1) == 0)
            {
                NLog.Warning("Server is not running.");
                return;
            }

            _cancellationTokenSource.Cancel();
            _listenerSocket.Close();
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