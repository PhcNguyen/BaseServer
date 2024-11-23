using System;
using System.Net.Sockets;
using System.Threading.Tasks;

using NServer.Core.Packet;
using NServer.Interfaces.Core.Network;

namespace NServer.Core.Network.SocketAsync
{
    internal class NSocket : INSocket, IDisposable
    {
        private readonly Guid _id;
        private readonly Socket _socket;
        private bool _disposed = false;
        private bool _hasError = false;

        private readonly SocketAsyncEventArgs _receiveEventArgs = new();
        private readonly SocketAsyncEventArgs _sendEventArgs = new();

        private readonly PacketProcessor _packetProcessor;
        private readonly BufferManager _bufferManager;

        public bool Disposed => _disposed;

        public NSocket(Socket socket, Guid id)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _id = id;

            _packetProcessor = new PacketProcessor(_id);
            _bufferManager = new BufferManager();

            _receiveEventArgs.Completed += OnReceiveCompleted;
            _sendEventArgs.Completed += OnSendCompleted;
        }

        public void StartReceiving() => ReceiveData();

        public void SendData(byte[] data) => SendDataInternal(data);

        public void SendData(Packets packet) => SendData(packet.ToByteArray());

        private void ReceiveData()
        {
            if (_socket == null || _hasError) return;

            byte[] buffer = _bufferManager.GetReceiveBuffer(_receiveEventArgs);
            _receiveEventArgs.SetBuffer(buffer, 0, buffer.Length);

            if (!_socket.ReceiveAsync(_receiveEventArgs))
            {
                OnReceiveCompleted(this, _receiveEventArgs);
            }
        }

        private void OnReceiveCompleted(object? sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                _ = HandleSocketErrorAsync(e.SocketError);
                return;
            }

            byte[] buffer = e.Buffer!;
            int length = BitConverter.ToInt32(buffer, 0);

            if (length <= 0 || length > buffer.Length)
            {
                _ = HandleSocketErrorAsync(SocketError.MessageSize);
                return;
            }

            byte[] data = new byte[length];
            Array.Copy(buffer, 0, data, 0, length);

            _packetProcessor.ProcessPacket(data);

            // Tiếp tục nhận dữ liệu
            ReceiveData();
        }

        private void SendDataInternal(byte[] data)
        {
            if (_socket == null || _hasError) return;

            byte[] buffer = _bufferManager.GetSendBuffer(_sendEventArgs, data.Length);
            Array.Copy(data, buffer, data.Length);
            _sendEventArgs.SetBuffer(buffer, 0, data.Length);

            if (!_socket.SendAsync(_sendEventArgs))
            {
                OnSendCompleted(this, _sendEventArgs);
            }
        }

        private void OnSendCompleted(object? sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                _ = HandleSocketErrorAsync(e.SocketError);
                return;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            lock (this)
            {
                if (_disposed) return;

                _disposed = true;
                _receiveEventArgs.Completed -= OnReceiveCompleted;
                _sendEventArgs.Completed -= OnSendCompleted;

                BufferManager.ReleaseBuffer(_receiveEventArgs);
                BufferManager.ReleaseBuffer(_sendEventArgs);

                _receiveEventArgs.Dispose();
                _sendEventArgs.Dispose();

                if (_socket.Connected)
                {
                    _socket.Shutdown(SocketShutdown.Both);
                }
                _socket.Dispose();
            }
        }

        private static bool IsRetryableError(SocketError error) =>
        error switch
        {
            SocketError.OperationAborted => true,
            SocketError.Interrupted => true,
            SocketError.MessageSize => true,
            SocketError.TimedOut => true,
            _ => false // Lỗi không thể tái thử
        };

        private async Task HandleSocketErrorAsync(SocketError socketError)
        {
            _hasError = true;
            const int maxRetryCount = 1;          // Số lần thử tối đa
            const int initialDelayMs = 1000;      // Thời gian trễ ban đầu (1 giây)
            const int maxDelayMs = 10000;         // Thời gian trễ tối đa (10 giây)
            int retryCount = 0;

            while (retryCount < maxRetryCount)
            {
                if (!IsRetryableError(socketError))
                {
                    break;
                }
                // Tính toán thời gian trễ (Exponential Backoff)
                int delayMs = Math.Min(initialDelayMs * (1 << retryCount), maxDelayMs);
                await Task.Delay(delayMs); // Đảm bảo chờ trước khi thực hiện lần retry tiếp theo

                retryCount++;
            }

            if (retryCount >= maxRetryCount)
            {
                Dispose();
            }
        }
    }
}