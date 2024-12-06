﻿using NPServer.Application.Main;
using NPServer.Core.Config;
using NPServer.Core.Helpers;
using NPServer.Core.Network.Firewall;
using NPServer.Core.Network.Listeners;
using NPServer.Infrastructure.Configuration;
using NPServer.Infrastructure.Logging;
using NPServer.Infrastructure.Services;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Application.Threading
{
    internal class ServerApp
    {
        private int _isRunning;
        private bool _isInMaintenanceMode;

        private SessionController _controller;
        private readonly SocketListener _networkListener;

        private CancellationTokenSource _ctokens;
        private readonly RequestLimiter _requestLimiter = Singleton.GetInstanceOfInterface<RequestLimiter>();
        private readonly NetworkConfig networkConfig = ConfigManager.Instance.GetConfig<NetworkConfig>();


        public static readonly string VersionInfo = $"Version {AssemblyHelper.GetAssemblyInformationalVersion()} | {(System.Diagnostics.Debugger.IsAttached ? "Debug" : "Release")}";

        public ServerApp()
        {
            _isRunning = 0;
            _isInMaintenanceMode = false;

            _ctokens = new CancellationTokenSource();
            _networkListener = new SocketListener(networkConfig.MaxConnections);
            _controller = new SessionController(networkConfig.Timeout, _ctokens.Token);
        }

        private void InitializeComponents()
        {
            _ctokens = new CancellationTokenSource();
            _controller = new SessionController(networkConfig.Timeout, _ctokens.Token);
        }

        public void Run()
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
            {
                NPLog.Instance.Warning("Server is already running.");
                return;
            }

            if (_networkListener.IsListening)
            {
                NPLog.Instance.Warning("Socket is already bound. Cannot start the server.");
                return;
            }

            // Khởi tạo lại các thành phần
            InitializeComponents();

            CancellationToken token = _ctokens.Token;

            _networkListener.StartListening(ipAddress: networkConfig.IP, port: networkConfig.Port);
            NPLog.Instance.Info<ServerApp>($"Starting network service at {networkConfig.IP}:{networkConfig.Port}");

            _ = Task.Run(async () =>
            {
                try
                {
                    await AcceptClientConnectionsAsync(token);
                }
                catch (OperationCanceledException)
                {
                    NPLog.Instance.Info("Accepting client connections was cancelled.");
                }
                catch (Exception ex)
                {
                    NPLog.Instance.Error(ex);
                    Shutdown();
                }
            }, token);
        }

        private async Task AcceptClientConnectionsAsync(CancellationToken token)
        {
            while (_isRunning == 1)
            {
                if (token.IsCancellationRequested)
                {
                    NPLog.Instance.Warning("Server stopping due to cancellation request.");
                    return;
                }

                if (_isInMaintenanceMode)
                {
                    NPLog.Instance.Warning("Server in maintenance mode.");
                    await Task.Delay(5000, token);
                    continue;
                }

                try
                {
                    if (!_networkListener.IsListening)
                    {
                        NPLog.Instance.Warning("Socket is no longer listening. Aborting connection accept.");
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
                    NPLog.Instance.Error<ServerApp>($"Socket error: {ex.SocketErrorCode}, Message: {ex.Message}");
                }
                catch (Exception ex)
                {
                    NPLog.Instance.Error<ServerApp>($"Unexpected error: {ex.Message}");
                }
            }
        }

        public void Shutdown()
        {
            if (Interlocked.CompareExchange(ref _isRunning, 0, 1) == 0)
            {
                NPLog.Instance.Warning("Server is not running.");
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
                    NPLog.Instance.Error<ServerApp>($"Error during disconnecting clients: {ex.Message}");
                }

                try
                {
                    _networkListener.StopListening();
                    _networkListener.Dispose(); // Đảm bảo giải phóng tài nguyên
                    NPLog.Instance.Info("Socket resources disposed.");
                }
                catch (Exception ex)
                {
                    NPLog.Instance.Error<ServerApp>($"Error during socket cleanup: {ex.Message}");
                }
            });

            NPLog.Instance.Info<ServerApp>("Server stopped successfully.");
        }

        public void Reset()
        {
            if (_isRunning == 1)
            {
                NPLog.Instance.Warning("Server is still stopping, waiting for the stop process to complete.");
                Task.Run(async () =>
                {
                    await Task.Delay(5000); // Đợi một khoảng thời gian trước khi thử lại
                    Reset();
                });
                return;
            }

            this.Shutdown();

            // Đảm bảo rằng tài nguyên socket được giải phóng hoàn toàn trước khi bắt đầu lại
            Task.Run(async () =>
            {
                await Task.Delay(2000); // Đợi thêm thời gian để giải phóng hoàn toàn
                this.Run();
                NPLog.Instance.Info("Server reset successfully.");
            });
        }

        public void SetMaintenanceMode(bool isMaintenance)
        {
            _isInMaintenanceMode = isMaintenance;
            NPLog.Instance.Info(isMaintenance ? "Server is now in maintenance mode." : "Server has exited maintenance mode.");
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