using System;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using NServer.Core.Network;
using NServer.Core.Packets;
using NServer.Core.Network.Firewall;
using NServer.Core.Interfaces.Session;
using NServer.Infrastructure.Helper;
using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Services;
using NServer.Infrastructure.Configuration;

namespace NServer.Core.Session
{
    /// <summary>
    /// Quản lý phiên làm việc của khách hàng.
    /// <para>
    /// Lớp này chịu trách nhiệm kết nối, xác thực, gửi/nhận dữ liệu và quản lý trạng thái của phiên làm việc từ phía khách hàng.
    /// </para>
    /// </summary>
    internal class SessionClient : ISessionClient, IAsyncDisposable
    {
        private bool _isDisposed;
        private readonly string _clientIp;

        private readonly ID36 _id;
        private readonly Socket _socket;
        private readonly TimeSpan _timeout;
        private readonly Stopwatch _activityTimer;
        private readonly SocketWriter _socketWriter;
        private readonly SocketReader _socketReader;

        private readonly ConnLimiter _connLimiter = Singleton.GetInstance<ConnLimiter>();
        private readonly PacketReceiver _packetReceiver = Singleton.GetInstance<PacketReceiver>();
        private readonly CancellationTokenSource _cts = new();

        /// <summary>
        /// Kiểm tra trạng thái kết nối của phiên làm việc.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Khóa phiên làm việc (dành cho mã hóa hoặc xác thực).
        /// </summary>
        public byte[] Key { get; set; } = [];

        /// <summary>
        /// Địa chỉ IP của khách hàng.
        /// </summary>
        public string IpAddress => _clientIp;

        /// <summary>
        /// ID duy nhất của phiên làm việc.
        /// </summary>
        public ID36 Id => _id;

        /// <summary>
        /// Socket kết nối của khách hàng.
        /// </summary>
        public Socket Socket => _socket;

        /// <summary>
        /// Khởi tạo phiên làm việc với socket cho sẵn.
        /// </summary>
        /// <param name="socket">Socket của khách hàng.</param>
        public SessionClient(Socket socket)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _clientIp = IPAddressHelper.GetClientIP(_socket);
            _id = ID36.NewId();
            _timeout = Setting.Timeout;
            _activityTimer = Stopwatch.StartNew();

            _socketWriter = new SocketWriter(_socket);
            _socketReader = new SocketReader(_socket, ProcessReceived);
        }

        /// <summary>
        /// Cập nhật thời gian hoạt động của phiên làm việc.
        /// </summary>
        public void UpdateLastActivityTime() => _activityTimer.Restart();

        /// <summary>
        /// Kiểm tra xem phiên làm việc có hết thời gian chờ không.
        /// </summary>
        /// <returns>True nếu phiên làm việc đã hết thời gian chờ, ngược lại False.</returns>
        public bool IsSessionTimedOut() => _activityTimer.Elapsed > _timeout;

        /// <summary>
        /// Kiểm tra xem socket có hợp lệ hay không.
        /// </summary>
        public bool IsSocketInvalid() => !_socket.Connected || _isDisposed || _socketReader.Disposed;

        /// <summary>
        /// Xác thực phiên làm việc dựa trên địa chỉ IP.
        /// </summary>
        /// <returns>True nếu phiên làm việc được phép kết nối, ngược lại False.</returns>
        public bool Authentication() => _connLimiter.IsConnectionAllowed(_clientIp);

        /// <summary>
        /// Kết nối và bắt đầu xử lý dữ liệu từ khách hàng.
        /// </summary>
        public async Task ConnectAsync()
        {
            try
            {
                IsConnected = true;

                if (string.IsNullOrEmpty(_clientIp) || IsSocketInvalid())
                {
                    NLog.Instance.Warning("Client address is invalid or Socket is not connected.");
                    await DisconnectAsync();
                    return;
                }

                NLog.Instance.Info($"Session {_id} connected to {_clientIp}");

                // Bắt đầu đọc gói tin không đồng bộ
                _socketReader.Receive(_cts.Token);
            }
            catch (Exception ex) when (ex is TimeoutException or IOException)
            {
                NLog.Instance.Error($"Connection error for {_clientIp}: {ex.Message}");
                await DisconnectAsync();
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Unexpected error for {_clientIp}: {ex.Message}");
                await DisconnectAsync();
            }
        }

        /// <summary>
        /// Ngắt kết nối phiên làm việc.
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (!IsConnected) return;

            IsConnected = false;

            try
            {
                _cts.Cancel();
                await DisposeAsync();
                _connLimiter.ConnectionClosed(_clientIp);
                NLog.Instance.Info($"Session {_id} disconnected from {_clientIp}");
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Error during disconnect: {ex.Message}");
            }
        }

        /// <summary>
        /// Xử lý gói tin nhận được từ khách hàng.
        /// </summary>
        private void ProcessReceived(byte[] data)
        {
            if (!_packetReceiver.AddPacket(_id, data))
            {
                NLog.Instance.Warning($"Failed to process received data for session {_id}");
            }
        }

        /// <summary>
        /// Gửi dữ liệu cho khách hàng.
        /// </summary>
        /// <param name="data">Dữ liệu cần gửi, có thể là mảng byte, chuỗi hoặc gói tin.</param>
        /// <returns>True nếu gửi thành công, ngược lại False.</returns>
        public async Task<bool> SendAsync(object data)
        {
            if (this.IsSocketInvalid()) return false;

            try
            {
                switch (data)
                {
                    case byte[] byteArray:
                        await _socketWriter.SendAsync(byteArray);
                        break;
                    case string str:
                        await _socketWriter.SendAsync(ConverterHelper.ToBytes(str));
                        break;
                    case Packet packet:
                        await _socketWriter.SendAsync(packet.ToByteArray());
                        break;
                    default:
                        throw new ArgumentException("Unsupported data type.");
                }

                return true;
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Error sending data: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Giải phóng tài nguyên của phiên làm việc.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_isDisposed) return;

            try
            {
                _cts.Cancel();
                await _socketWriter.DisposeAsync();
                await _socketReader.DisposeAsync();
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Error disposing session: {ex.Message}");
            }
            finally
            {
                _socket.Dispose();
                _cts.Dispose();
                _isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}