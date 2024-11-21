using NServer.Infrastructure.Configuration;
using System.Net.Sockets;
using System.Diagnostics;
using NServer.Infrastructure.Helper;
using NServer.Interfaces.Core.Network;
using NServer.Core.Logging;

namespace NServer.Core.Network
{
    /// <summary>
    /// Lớp quản lý một phiên làm việc của khách hàng.
    /// </summary>
    internal class Session : ISession, IDisposable
    {
        private bool _disposed;
        private readonly Guid _id;
        private readonly Socket _socket;
        private readonly NSocket _socketAsync;
        private readonly Stopwatch _activityTimer;
        private readonly TimeSpan _timeout = Setting.Timeout;

        /// <summary>
        /// Trạng thái kết nối của phiên làm việc.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Khóa phiên làm việc.
        /// </summary>
        public byte[] Key { get; private set; }

        /// <summary>
        /// Địa chỉ IPv4 của khách hàng.
        /// </summary>
        public string Ip { get; private set; } = string.Empty;

        /// <summary>
        /// ID của phiên làm việc.
        /// </summary>
        public Guid Id => _id;

        /// <summary>
        /// Socket của phiên làm việc.
        /// </summary>
        public Socket Socket => _socket;

        /// <summary>
        /// Khởi tạo phiên làm việc của khách hàng.
        /// </summary>
        /// <param name="socket">Socket kết nối của phiên làm việc.</param>
        public Session(Socket socket)
        {
            _activityTimer = Stopwatch.StartNew();
            _id = Guid.NewGuid();

            _socket = socket;
            _socketAsync = new NSocket(socket, _id);

            Key = Generator.K256();
            Ip = IPAddressHelper.GetClientIP(_socket);
        }

        /// <summary>
        /// Cập nhật thời gian hoạt động cuối cùng của phiên làm việc.
        /// </summary>
        public void UpdateLastActivityTime() => _activityTimer.Restart();

        /// <summary>
        /// Kiểm tra xem phiên làm việc có bị hết thời gian không.
        /// </summary>
        /// <returns>Trả về true nếu phiên làm việc hết thời gian, ngược lại false.</returns>
        public bool IsSessionTimedOut() => _activityTimer.Elapsed > _timeout;

        /// <summary>
        /// Kết nối phiên làm việc.
        /// </summary>
        public async Task Connect()
        {
            try
            {
                IsConnected = true;
                if (string.IsNullOrEmpty(Ip) || !_socket.Connected)
                {
                    NLog.Warning("Client address is invalid or Socket is not connected.");
                    await Disconnect();
                    return;
                }

                NLog.Info($"Session {_id} connected to {Ip}");

                _socketAsync.StartReceiving();
            }
            catch (TimeoutException tex)
            {
                NLog.Error($"Timeout while establishing connection for {Ip}: {tex.Message}");
                await Disconnect();
            }
            catch (IOException ioex)
            {
                NLog.Error($"I/O error while setting up client stream for {Ip}: {ioex.Message}");
                await Disconnect();
            }
            catch (Exception ex)
            {
                NLog.Error($"Unexpected error while connecting client {Ip}: {ex.Message}");
                await Disconnect();
            }
        }

        /// <summary>
        /// Ngắt kết nối phiên làm việc.
        /// </summary>
        public async Task Disconnect()
        {
            if (!IsConnected) return;

            try
            {
                IsConnected = false;

                await Task.Run(() => Dispose());

                NLog.Info($"Session {_id} disconnected from {Ip}");
            }
            catch (ObjectDisposedException ex)
            {
                NLog.Warning($"Attempted to dispose already disposed objects: {ex.Message}");
            }
            catch (Exception ex)
            {
                NLog.Error(ex);
            }
        }

        public bool Send(object data)
        {
            if (!_socket.Connected) return false;
            switch (data)
            {
                case byte[] byteArray:
                    _socketAsync.SendData(byteArray);
                    break;
                case string str:
                    _socketAsync.SendData(ByteConverter.ToBytes(str));
                    break;
                default:
                    throw new ArgumentException("Unsupported data type.");
            }

            return true;
        }

        /// <summary>
        /// Giải phóng tài nguyên được sử dụng bởi <see cref="Session"/>.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _socketAsync.Dispose();
            _disposed = true;
            _socket.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
