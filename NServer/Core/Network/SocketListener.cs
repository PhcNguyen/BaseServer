using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;

using NServer.Infrastructure.Helper;
using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Configuration;

namespace NServer.Core.Network
{
    /// <summary>
    /// SocketListener là lớp quản lý việc lắng nghe kết nối TCP đến server.
    /// </summary>
    internal class SocketListener : IDisposable
    {
        private Socket _listenerSocket;
        private readonly int _maxConnections = Setting.MaxConnections;

        public bool IsListening => _listenerSocket?.IsBound == true;

        public SocketListener()
        {
            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ConfigureSocket(_listenerSocket);
        }

        /// <summary>
        /// Bắt đầu lắng nghe các kết nối đến.
        /// </summary>
        /// <param name="ipAddress">Địa chỉ IP để lắng nghe, nếu null thì sử dụng tất cả địa chỉ.</param>
        /// <param name="port">Cổng để lắng nghe.</param>
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
                IPAddress parsedIPAddress = string.IsNullOrEmpty(ipAddress) ? IPAddress.Any : IPAddressHelper.ParseIPAddress(ipAddress);
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

        /// <summary>
        /// Dừng việc lắng nghe và đóng socket.
        /// </summary>
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

        /// <summary>
        /// Đặt lại listener, đóng socket và giải phóng tài nguyên.
        /// </summary>
        public void ResetListener()
        {
            StopListening();  // Gọi StopListening để đảm bảo socket đã đóng đúng cách.
            Dispose();        // Giải phóng tài nguyên của socket.

            // Khởi tạo lại socket listener để có thể sử dụng lại
            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ConfigureSocket(_listenerSocket);
        }

        /// <summary>
        /// Chấp nhận kết nối client một cách bất đồng bộ.
        /// </summary>
        public async Task<Socket?> AcceptClientAsync(CancellationToken _token)
        {
            try
            {
                return await _listenerSocket.AcceptAsync(_token);
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

        /// <summary>
        /// Giải phóng tài nguyên khi không còn cần thiết.
        /// </summary>
        public void Dispose()
        {
            try
            {
                _listenerSocket?.Dispose();
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Error disposing socket resources: {ex.Message}");
            }
        }

        /// <summary>
        /// Cấu hình các tùy chọn cho socket.
        /// </summary>
        private static void ConfigureSocket(Socket socket)
        {
            socket.Blocking = Setting.Blocking; // Kiểm soát chế độ blocking.
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); // Cho phép tái sử dụng địa chỉ.
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, Setting.KeepAlive); // Tùy chọn giữ kết nối sống.
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, Setting.ReuseAddress); // Tùy chọn tái sử dụng địa chỉ.
        }
    }
}