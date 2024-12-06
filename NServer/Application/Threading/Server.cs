using NPServer.Application.Main;
using NPServer.Core.Helpers;
using NPServer.Core.Network.Firewall;
using NPServer.Core.Network.Listeners;
using NPServer.Core.Services;
using NPServer.Infrastructure.Configuration;
using NPServer.Infrastructure.Logging;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Application.Threading
{
    internal class Server
    {
        private int _isRunning;
        private bool _isInMaintenanceMode;

        private SessionController _controller;
        private readonly SocketListener _networkListener;

        private CancellationTokenSource _ctokens;
        private readonly RequestLimiter _requestLimiter = Singleton.GetInstance<RequestLimiter>();

        public Server()
        {
            _isRunning = 0;
            _isInMaintenanceMode = false;

            _ctokens = new CancellationTokenSource();
            _networkListener = new SocketListener(Setting.MaxConnections);
            _controller = new SessionController(_ctokens.Token);
        }

        private void InitializeComponents()
        {
            _ctokens = new CancellationTokenSource();
            _controller = new SessionController(_ctokens.Token);
        }

        public void StartServer()
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
            {
                NLog.Instance.Warning("Server is already running.");
                return;
            }

            if (_networkListener.IsListening)
            {
                NLog.Instance.Warning("Socket is already bound. Cannot start the server.");
                return;
            }

            // Khởi tạo lại các thành phần
            InitializeComponents();

            var token = _ctokens.Token;

            _networkListener.StartListening(ipAddress: null, port: Setting.Port);

            _ = Task.Run(async () =>
            {
                try
                {
                    await AcceptClientConnectionsAsync(token);
                }
                catch (OperationCanceledException)
                {
                    NLog.Instance.Info("Accepting client connections was cancelled.");
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
                    if (!_networkListener.IsListening)
                    {
                        NLog.Instance.Warning("Socket is no longer listening. Aborting connection accept.");
                        break;
                    }

                    Socket? acceptSocket = await _networkListener.AcceptClientAsync(token);

                    if (acceptSocket == null) continue;

                    if (!_requestLimiter.IsAllowed(NetworkHelper.GetClientIP(acceptSocket)))
                    {
                        acceptSocket.Close();
                        continue;
                    }

                    _controller.AcceptClient(acceptSocket);
                }
                catch (SocketException ex)
                {
                    NLog.Instance.Error<Server>($"Socket error: {ex.SocketErrorCode}, Message: {ex.Message}");
                }
                catch (Exception ex)
                {
                    NLog.Instance.Error<Server>($"Unexpected error: {ex.Message}");
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

            _ctokens.Cancel();

            Task.Run(async () =>
            {
                try
                {
                    await _controller.DisconnectAllClientsAsync();
                }
                catch (Exception ex)
                {
                    NLog.Instance.Error<Server>($"Error during disconnecting clients: {ex.Message}");
                }

                try
                {
                    _networkListener.StopListening();
                    _networkListener.Dispose(); // Đảm bảo giải phóng tài nguyên
                    NLog.Instance.Info("Socket resources disposed.");
                }
                catch (Exception ex)
                {
                    NLog.Instance.Error<Server>($"Error during socket cleanup: {ex.Message}");
                }
            });

            NLog.Instance.Info("Server stopped successfully.");
        }

        public void ResetServer()
        {
            if (_isRunning == 1)
            {
                NLog.Instance.Warning("Server is still stopping, waiting for the stop process to complete.");
                Task.Run(async () =>
                {
                    await Task.Delay(5000); // Đợi một khoảng thời gian trước khi thử lại
                    ResetServer();
                });
                return;
            }

            StopServer();

            // Đảm bảo rằng tài nguyên socket được giải phóng hoàn toàn trước khi bắt đầu lại
            Task.Run(async () =>
            {
                await Task.Delay(2000); // Đợi thêm thời gian để giải phóng hoàn toàn
                StartServer();
                NLog.Instance.Info("Server reset successfully.");
            });
        }

        public void SetMaintenanceMode(bool isMaintenance)
        {
            _isInMaintenanceMode = isMaintenance;
            NLog.Instance.Info(isMaintenance ? "Server is now in maintenance mode." : "Server has exited maintenance mode.");
        }

        public int GetActiveConnections()
        {
            return _controller.ActiveSessions();
        }

        public bool IsServerRunning()
        {
            return _isRunning == 1;
        }

        public void CancelOperation()
        {
            _ctokens.Cancel();
            _ctokens.Dispose(); // Disposes the token source
        }
    }
}