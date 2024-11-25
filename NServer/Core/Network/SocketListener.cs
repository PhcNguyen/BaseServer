using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;

using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Configuration;

namespace NServer.Core.Network
{
    internal class SocketListener : IDisposable
    {
        private Socket _listenerSocket;
        private readonly int _maxConnections = Setting.MaxConnections;

        public bool IsListening => _listenerSocket?.IsBound ?? false;
        public bool IsSocketBound => _listenerSocket.IsBound;

        public SocketListener()
        {
            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ConfigureSocket(_listenerSocket);
        }

        private static void ConfigureSocket(Socket socket)
        {
            socket.Blocking = Setting.Blocking;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, Setting.KeepAlive);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, Setting.ReuseAddress);  
        }

        public void StartListening(string? ipAddress, int port)
        {
            if (_listenerSocket.IsBound)
            {
                NLog.Instance.Warning("Socket is already bound. StartListening cannot be called multiple times.");
                throw new InvalidOperationException("Socket is already bound.");
            }

            if (port < 0 || port > 65535)
            {
                NLog.Instance.Error($"Invalid port number: {port}");
                throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 0 and 65535.");
            }

            try
            {
                IPAddress parsedIPAddress = string.IsNullOrEmpty(ipAddress) ? IPAddress.Any : ParseIPAddress(ipAddress);
                var localEndPoint = new IPEndPoint(parsedIPAddress, port);

                _listenerSocket.Bind(localEndPoint);
                _listenerSocket.Listen(_maxConnections);

                if (port == 0)
                {
                    var selectedPort = ((IPEndPoint)_listenerSocket.LocalEndPoint!).Port;
                    NLog.Instance.Info($"Listening on dynamically selected port: {selectedPort}");
                    return;
                }

                NLog.Instance.Info($"Listening on {localEndPoint}");
            }
            catch (Exception ex) when (ex is FormatException or SocketException)
            {
                NLog.Instance.Error($"Error starting listener: {ex.Message}");
                throw new InvalidOperationException("Failed to start listening.", ex);
            }
        }

        public void StopListening()
        {
            if (!_listenerSocket.IsBound)
            {
                NLog.Instance.Warning("StopListening called but socket is not bound.");
                return;
            }

            try
            {
                if (_listenerSocket.Connected)
                {
                    _listenerSocket.Shutdown(SocketShutdown.Both);
                }

                _listenerSocket.Close();
            }
            catch (SocketException ex)
            {
                NLog.Instance.Error($"Error shutting down socket: {ex.Message}");
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Unexpected error while shutting down socket: {ex.Message}");
            }
        }

        public void ResetListener()
        {
            StopListening();  // Gọi StopListening để đảm bảo socket đã đóng đúng cách.
            Dispose();        // Giải phóng tài nguyên của socket.

            // Khởi tạo lại socket listener để có thể sử dụng lại
            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ConfigureSocket(_listenerSocket);
            NLog.Instance.Info("SocketListener has been reset.");
        }

        public async Task<Socket?> AcceptClientAsync(CancellationToken cancellationToken)
        {
            try
            {
                var acceptTask = Task.Factory.FromAsync(_listenerSocket.BeginAccept, _listenerSocket.EndAccept, null);

                if (await Task.WhenAny(acceptTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false) == acceptTask)
                {
                    return acceptTask.Result;  // Kết nối thành công
                }
                else
                {
                    NLog.Instance.Info("AcceptClientAsync operation was cancelled due to cancellation token.");
                    return null;  // Nếu task bị hủy do token
                }
            }
            catch (ObjectDisposedException)
            {
                NLog.Instance.Warning("Socket was closed during Accept operation.");
                return null;
            }
            catch (OperationCanceledException)
            {
                NLog.Instance.Info("AcceptClientAsync was cancelled due to cancellation token.");
                return null;
            }
            catch (ThreadAbortException)
            {
                NLog.Instance.Error("Thread was aborted while waiting for Accept.");
                return null;
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Error accepting client: {ex.Message}");
                return null;
            }
        }

        public void Dispose()
        {
            try
            {
                _listenerSocket.Dispose();
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Error disposing socket resources: {ex.Message}");
            }
        }

        private static IPAddress ParseIPAddress(string ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress, out var parsedIPAddress))
            {
                NLog.Instance.Error($"Invalid IP address format: {ipAddress}");
                throw new ArgumentException("The provided IP address is not valid.", nameof(ipAddress));
            }
            return parsedIPAddress;
        }
    }
}