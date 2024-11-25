using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using NServer.Core.Network;
using NServer.Application.Main;
using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Configuration;

namespace NServer.Application.Threading
{
    internal class Server
    {
        private int _isRunning;
        private bool _isInMaintenanceMode;
        
        private readonly NetworkListener _networkListener;
        private readonly SessionController _sessionController;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public Server()
        {
            _isRunning = 0;
            _isInMaintenanceMode = false;

            _networkListener = new NetworkListener();
            _cancellationTokenSource = new CancellationTokenSource();
            _sessionController = new SessionController(_cancellationTokenSource.Token);
        }

        public void StartServer()
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
            {
                NLog.Instance.Warning("Server is already running.");
                return;
            }

            var token = _cancellationTokenSource.Token;
            _networkListener.StartListening(ipAddress: null, port: Setting.Port);

            _ = Task.Run(async () =>
            {
                try
                {
                    await AcceptClientConnectionsAsync(token);
                }
                catch (Exception ex)
                {
                    NLog.Instance.Error(ex);
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
                    NLog.Instance.Warning("Server stopping due to cancellation request.");
                    return;
                }

                if (_isInMaintenanceMode)
                {
                    NLog.Instance.Warning("Server in maintenance mode.");
                    await Task.Delay(5000, token);
                    continue;
                }

                try
                {
                    Socket? acceptSocket = await _networkListener.AcceptClientAsync(token);
                    if (acceptSocket == null) continue;

                    await _sessionController.AcceptClientAsync(acceptSocket);
                }
                catch (SocketException ex)
                {
                    NLog.Instance.Error($"Socket error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    NLog.Instance.Error($"Unexpected error: {ex.Message}");
                }
            }
        }

        public void StopServer()
        {
            if (Interlocked.CompareExchange(ref _isRunning, 0, 1) == 0)
            {
                NLog.Instance.Warning("Server is not running.");
                return;
            }

            _cancellationTokenSource.Cancel();

            Task.Run(async () =>
            {
                try
                {
                    await _sessionController.DisconnectAllClientsAsync();
                }
                catch (Exception ex)
                {
                    NLog.Instance.Error($"Error during disconnecting clients: {ex.Message}");
                }
            });

            _networkListener.StopListening();
            NLog.Instance.Info("Server stopped successfully.");
        }

        public void ResetServer()
        {
            StopServer();
            StartServer();
            NLog.Instance.Info("Server reset successfully.");
        }

        public void SetMaintenanceMode(bool isMaintenance)
        {
            _isInMaintenanceMode = isMaintenance;
            NLog.Instance.Info(isMaintenance ? "Server is now in maintenance mode." : "Server has exited maintenance mode.");
        }
    }
}