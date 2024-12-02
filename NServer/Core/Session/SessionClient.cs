using NServer.Core.Helper;
using NServer.Core.Interfaces.Session;
using NServer.Core.Network.IO;
using NServer.Core.Packets;
using NServer.Infrastructure.Configuration;
using NServer.Infrastructure.Helper;
using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Security;
using NServer.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NServer.Core.Session
{
    /// <summary>
    /// Quản lý phiên làm việc của khách hàng.
    /// <para>
    /// Lớp này chịu trách nhiệm kết nối, xác thực, gửi/nhận dữ liệu và quản lý trạng thái của phiên làm việc từ phía khách hàng.
    /// </para>
    /// </summary>
    public class SessionClient : ISessionClient
    {
        private bool _isDisposed;
        private readonly string _clientIp;
        private Action<UniqueId, byte[]>? _processdata;

        private readonly UniqueId _id;
        private readonly Socket _socket;
        private readonly TimeSpan _timeout;
        private readonly Stopwatch _activityTimer;
        private readonly SocketWriter _socketWriter;
        private readonly SocketReader _socketReader;
        private readonly Queue<Task> _sendQueue = new();
        private readonly CancellationTokenSource _cts = new();

        /// <summary>
        /// Kiểm tra trạng thái kết nối của phiên làm việc.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Khóa phiên làm việc (dành cho mã hóa hoặc xác thực).
        /// </summary>
        public byte[] Key { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Địa chỉ IP của khách hàng.
        /// </summary>
        public string IpAddress => _clientIp;

        public bool Authenticator { get; set; } = false;

        /// <summary>
        /// ID duy nhất của phiên làm việc.
        /// </summary>
        public UniqueId Id => _id;

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
            _clientIp = NetworkHelper.GetClientIP(_socket);

            _id = UniqueId.NewId();
            _timeout = Setting.Timeout;
            _activityTimer = Stopwatch.StartNew();

            _socketWriter = new SocketWriter(_socket);
            _socketReader = new SocketReader(_socket);

            _socketReader.DataReceived += OnDataReceived!;
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
            }
            catch (Exception ex) when (ex is TimeoutException or IOException)
            {
                NLog.Instance.Error<SessionClient>($"Connection error for {_clientIp}: {ex.Message}");
                await DisconnectAsync();
            }
            catch (Exception ex)
            {
                NLog.Instance.Error<SessionClient>($"Unexpected error for {_clientIp}: {ex.Message}");
                await DisconnectAsync();
            }
        }

        public async Task ReconnectAsync()
        {
            if (!IsConnected)
            {
                int retries = 3;
                TimeSpan delay = TimeSpan.FromSeconds(2);

                while (retries > 0)
                {
                    try
                    {
                        NLog.Instance.Info("Đang thử kết nối lại...");
                        await ConnectAsync();
                        return; // Kết nối thành công
                    }
                    catch (Exception ex)
                    {
                        NLog.Instance.Error<SessionClient>($"Lần thử kết nối lại thất bại: {ex.Message}");
                        retries--;
                        if (retries > 0)
                        {
                            await Task.Delay(delay); // Tăng dần độ trễ
                            delay = delay.Add(delay); // Tăng gấp đôi độ trễ
                        }
                    }
                }
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
                await _cts.CancelAsync();
                await DisposeAsync();

                NLog.Instance.Info<SessionClient>($"Session {_id} disconnected from {_clientIp}");
            }
            catch (Exception ex)
            {
                NLog.Instance.Error<SessionClient>($"Error during disconnect: {ex.Message}");
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
                Task sendTask = data switch
                {
                    byte[] byteArray => _socketWriter.SendAsync(Crc32Checksum.AddCrc32(byteArray)),
                    string str => _socketWriter.SendAsync(Crc32Checksum.AddCrc32(ConverterHelper.ToBytes(str))),
                    Packet packet => _socketWriter.SendAsync(Crc32Checksum.AddCrc32(packet.ToByteArray())),
                    _ => throw new ArgumentException("Unsupported data type.")
                };

                // Thêm task gửi vào hàng đợi
                _sendQueue.Enqueue(sendTask);

                // Kiểm tra và gửi lần lượt
                await ProcessSendQueueAsync();
                return true;
            }
            catch (Exception ex)
            {
                NLog.Instance.Error<SessionClient>($"Error sending data: {ex.Message}");
                return false;
            }
        }

        public void Receive(Action<UniqueId, byte[]> processdata)
        {
            _processdata = processdata;
            _socketReader.Receive(_cts.Token);
        }

        private void OnDataReceived(object sender, SocketReceivedEventArgs e)
        {
            if (_processdata == null) return;
            bool isValid = Crc32Checksum.VerifyCrc32(e.Data, out byte[]? originalData);

            if (isValid && originalData != null)
            {
                _processdata(_id, originalData);
            }
        }

        private async Task ProcessSendQueueAsync()
        {
            while (_sendQueue.Count > 0)
            {
                var task = _sendQueue.Dequeue();
                try
                {
                    // Chờ cho task hoàn thành mà không chặn luồng
                    await task.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    NLog.Instance.Error<SessionClient>($"Lỗi trong quá trình gửi dữ liệu: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Giải phóng tài nguyên của phiên làm việc.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_isDisposed) return;

            // Dispose managed resources
            await _cts.CancelAsync();
            _socketReader.DataReceived -= OnDataReceived!;

            _socketWriter.Dispose();
            await _socketReader.DisposeAsync();

            _socket.Dispose();
            _cts.Dispose();

            _isDisposed = true;
        }

        // Finalizer if necessary for unmanaged resources
        ~SessionClient()
        {
            DisposeAsync().AsTask().Wait(); // Ensure that DisposeAsync is called if needed
        }
    }
}