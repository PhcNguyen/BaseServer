using System;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading.Tasks;

using NServer.Core.Network;
using NServer.Core.Packets;
using NServer.Core.Packets.Utils;
using NServer.Core.Network.Firewall;
using NServer.Core.Interfaces.Session;
using NServer.Infrastructure.Helper;
using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Services;
using NServer.Infrastructure.Configuration;

namespace NServer.Core.Session
{
    /// <summary>
    /// Lớp quản lý một phiên làm việc của khách hàng.
    /// </summary>
    internal class SessionClient : ISessionClient, IDisposable
    {
        private bool _isDisposed;
        private readonly string _clientIp;

        private readonly SessionID _id;
        private readonly Socket _socket;
        private readonly TimeSpan _timeout;
        private readonly Stopwatch _activityTimer;
        private readonly SocketWriter _socketWriter;
        private readonly SocketReader _socketReader;

        private readonly ConnLimiter _connLimiter = Singleton.GetInstance<ConnLimiter>();
        private readonly PacketReceiver _packetReceiver = Singleton.GetInstance<PacketReceiver>();

        /// <summary>
        /// Trạng thái kết nối của phiên làm việc.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Khóa phiên làm việc.
        /// </summary>
        public byte[] Key { get; set; } = [];

        /// <summary>
        /// Địa chỉ IP của khách hàng.
        /// </summary>
        public string IpAddress => _clientIp;

        /// <summary>
        /// ID của phiên làm việc.
        /// </summary>
        public SessionID Id => _id;

        /// <summary>
        /// Socket của phiên làm việc.
        /// </summary>
        public Socket Socket => _socket;

        /// <summary>
        /// Khởi tạo phiên làm việc của khách hàng.
        /// </summary>
        /// <param name="socket">Socket kết nối của phiên làm việc.</param>
        public SessionClient(Socket socket)
        {
            _socket = socket;
            _clientIp = IPAddressHelper.GetClientIP(_socket);

            _id = SessionID.NewId();
            _timeout = Setting.Timeout;
            _activityTimer = Stopwatch.StartNew();

            _socketWriter = new SocketWriter(_socket);
            _socketReader = new SocketReader(_socket, this.ProcessReceivedData);
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
        /// Kiểm tra xem socket có bị dispose chưa.
        /// </summary>
        /// <returns>Trả về true nếu socket đã bị dispose, ngược lại false.</returns>
        public bool IsSocketDisposed()
        {
            try
            {
                // Kiểm tra trạng thái kết nối và dispose của socket
                return !_socket.Connected
                    || _isDisposed
                    || _socket == null
                    || _socketReader.Disposed;
            }
            catch (ObjectDisposedException)
            {
                // Socket đã bị dispose
                return true;
            }
        }

        public bool Authentication()
        {
            if (_connLimiter.IsConnectionAllowed(_clientIp)) return true;
            return false;
        }

        /// <summary>
        /// Kết nối phiên làm việc.
        /// </summary>
        public async Task Connect()
        {
            try
            {
                IsConnected = true;
                if (string.IsNullOrEmpty(_clientIp) || !_socket.Connected)
                {
                    NLog.Instance.Warning("Client address is invalid or Socket is not connected.");
                    await Disconnect();
                    return;
                }

                NLog.Instance.Info($"Session {_id} connected to {_clientIp}");

                _socketReader.Receive();
            }
            catch (TimeoutException tex)
            {
                NLog.Instance.Error($"Timeout while establishing connection for {_clientIp}: {tex.Message}");
                await Disconnect();
            }
            catch (IOException ioex)
            {
                NLog.Instance.Error($"I/O error while setting up client stream for {_clientIp}: {ioex.Message}");
                await Disconnect();
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Unexpected error while connecting client {_clientIp}: {ex.Message}");
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

                _connLimiter.ConnectionClosed(_clientIp);

                NLog.Instance.Info($"Session {_id} disconnected from {_clientIp}");
            }
            catch (ObjectDisposedException ex)
            {
                NLog.Instance.Warning($"Attempted to dispose already disposed objects: {ex.Message}");
            }
            catch (Exception ex)
            {
                NLog.Instance.Error(ex);
            }
        }

        private void ProcessReceivedData(byte[] data)
        {
            if (data.Length < 8)
            {
                return;
            }

            try
            {
                Packet packet = PacketExtensions.FromByteArray(data);

                packet.SetID(_id);

                _packetReceiver.AddPacket(packet);
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"{_id} - Error processing packet: {ex.Message}");
            }
        }

        public async Task<bool> Send(object data)
        {
            if (!_socket.Connected) return false;
            switch (data)
            {
                case byte[] byteArray:
                    await _socketWriter.WriteAsync(byteArray);
                    break;
                case string str:
                    await _socketWriter.WriteAsync(ConverterHelper.ToBytes(str));
                    break;
                default:
                    throw new ArgumentException("Unsupported data type.");
            }

            return true;
        }

        /// <summary>
        /// Giải phóng tài nguyên được sử dụng bởi <see cref="SessionClient"/>.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            _socketWriter.Dispose();
            _socketReader.Dispose();
            _isDisposed = true;
            _socket.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}