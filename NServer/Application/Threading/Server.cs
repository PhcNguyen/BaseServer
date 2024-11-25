using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using NServer.Core.Network;
using NServer.Application.Main;
using NServer.Core.Network.Firewall;
using NServer.Infrastructure.Helper;
using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Services;
using NServer.Infrastructure.Configuration;
using NServer.Core.Network.BufferPool;

namespace NServer.Application.Threading
{
    internal class Server : IDisposable
    {
        private bool _disposed;
        private int _isRunning; 
        private bool _isInMaintenanceMode;

        private Controller _controller;
        private readonly SocketListener _networkListener;

        private CancellationTokenSource _cancellationTokenSource;

        private readonly MultiSizeBuffer _multiSizeBuffer = Singleton.GetInstance<MultiSizeBuffer>();
        private readonly RequestLimiter _requestLimiter = Singleton.GetInstance<RequestLimiter>(() =>
            new RequestLimiter(Setting.RateLimit, Setting.ConnectionLockoutDuration));

        public Server()
        {
            _isRunning = 0;
            _disposed = false;
            _isInMaintenanceMode = false;

            _multiSizeBuffer.AllocateBuffers();
            _networkListener = new SocketListener();
            _cancellationTokenSource = new CancellationTokenSource();
            _controller = new Controller(_cancellationTokenSource.Token);
        }

        private void InitializeComponents()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _controller = new Controller(_cancellationTokenSource.Token);
        }

        public void StartServer()
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
            {
                NLog.Instance.Warning("Server is already running.");
                return;
            }

            if (_networkListener.IsSocketBound)
            {
                NLog.Instance.Warning("Socket is already bound. Cannot start the server.");
                return;
            }

            // Khởi tạo lại các thành phần
            InitializeComponents();

            var token = _cancellationTokenSource.Token;

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

                    if (!_requestLimiter.IsAllowed(IPAddressHelper.GetClientIP(acceptSocket)))
                    {
                        Console.WriteLine($"Ban {acceptSocket.SocketType}");
                        acceptSocket.Close();
                        continue;
                    }

                    await _controller.AcceptClientAsync(acceptSocket);
                }
                catch (SocketException ex)
                {
                    NLog.Instance.Error($"Socket error: {ex.SocketErrorCode}, Message: {ex.Message}");
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
                    await _controller.DisconnectAllClientsAsync();
                }
                catch (Exception ex)
                {
                    NLog.Instance.Error($"Error during disconnecting clients: {ex.Message}");
                }

                try
                {
                    _networkListener.StopListening();
                    _networkListener.Dispose(); // Đảm bảo giải phóng tài nguyên
                    NLog.Instance.Info("Socket resources disposed.");
                }
                catch (Exception ex)
                {
                    NLog.Instance.Error($"Error during socket cleanup: {ex.Message}");
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

        public void Dispose()
        {
            if (_disposed)
                return;

            // Hủy bỏ các tài nguyên được quản lý
            _cancellationTokenSource?.Cancel();
            _networkListener?.Dispose();

            _disposed = true;
        }
    }
}