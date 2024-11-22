using NServer.Core.Packet;
using NServer.Core.Packet.Utils;
using NServer.Core.Network.Buffers;
using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Services;

using System.Net.Sockets;

namespace NServer.Core.Network
{
    internal class NSocket : IDisposable
    {
        private readonly PacketContainer _packetContainer = Singleton.GetInstance<PacketContainer>();
        private readonly MultiSizeBuffer _multiSizeBuffer = Singleton.GetInstance<MultiSizeBuffer>();
        private readonly int _bufferDefaultSize = 256;

        private readonly Socket _socket;
        private readonly Guid _id;
        private bool _disposed = false;
        private bool _hasError = false;

        private readonly SocketAsyncEventArgs _receiveEventArgs = new();
        private readonly SocketAsyncEventArgs _sendEventArgs = new();

        public NSocket(Socket socket, Guid id)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _id = id;

            _receiveEventArgs.Completed += OnReceiveCompleted;
            _sendEventArgs.Completed += OnSendCompleted;
        }

        private void ProcessPacket(Guid sessionId, byte[] data)
        {
            if (data.Length < 8) return;

            Packets packet = PacketExtensions.FromByteArray(data);
            if (packet != null)
            {
                if (!packet.IsValid()) return;
                _packetContainer.AddPacket(sessionId, packet);
            }
        }

        public void StartReceiving() => ReceiveData();

        public void SendData(byte[] data) => SendDataInternal(data);

        public void SendData(Packets packet) => SendData(packet.ToByteArray());

        private void ReceiveData()
        {
            if (_socket == null || _hasError) return;

            byte[] buffer = GetReceiveBuffer();
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
                HandleSocketError(e.SocketError);
                return;
            }

            byte[] buffer = e.Buffer!;
            int length = BitConverter.ToInt32(buffer, 0);

            if (length <= 0 || length > buffer.Length)
            {
                HandleSocketError(SocketError.MessageSize);
                return;
            }

            byte[] data = new byte[length];
            Array.Copy(buffer, 0, data, 0, length);

            ProcessPacket(_id, data);

            // Tiếp tục nhận dữ liệu
            ReceiveData();
        }

        private void SendDataInternal(byte[] data)
        {
            if (_socket == null || _hasError) return;

            byte[] buffer = GetSendBuffer(data.Length);
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
                HandleSocketError(e.SocketError);
                return;
            }
        }

        private byte[] GetReceiveBuffer()
        {
            if (_receiveEventArgs.UserToken is byte[] existingBuffer)
            {
                return existingBuffer;
            }

            byte[] buffer = _multiSizeBuffer.RentBuffer(_bufferDefaultSize);
            _receiveEventArgs.UserToken = buffer;
            return buffer;
        }


        private byte[] GetSendBuffer(int length)
        {
            if (_sendEventArgs.UserToken is byte[] existingBuffer && existingBuffer.Length >= length)
            {
                return existingBuffer;
            }

            byte[] buffer = _multiSizeBuffer.RentBuffer(length);
            _sendEventArgs.UserToken = buffer;
            return buffer;
        }


        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _socket?.Close();

            // Kiểm tra null trước khi trả lại bộ đệm
            if (_receiveEventArgs.UserToken is byte[] receiveBuffer)
            {
                _multiSizeBuffer.ReturnBuffer(receiveBuffer);
            }

            if (_sendEventArgs.UserToken is byte[] sendBuffer)
            {
                _multiSizeBuffer.ReturnBuffer(sendBuffer);
            }
        }

        private void HandleSocketError(SocketError socketError)
        {
            _hasError = true;

            switch (socketError)
            {
                case SocketError.OperationAborted:
                case SocketError.Interrupted:
                    NLog.Warning($"{_id} - Operation aborted or interrupted. Continuing...");
                    break;

                case SocketError.MessageSize:
                    NLog.Warning($"{_id} - Message size error. Trying again...");
                    ReceiveData();
                    break;

                case SocketError.TimedOut:
                    NLog.Warning($"{_id} - Socket timeout. Attempting to reconnect or continue...");
                    _socket?.Close();
                    break;

                case SocketError.AddressNotAvailable:
                    NLog.Warning($"{_id} - Address not available. Closing connection.");
                    _socket?.Close();
                    break;

                case SocketError.Shutdown:
                    NLog.Error($"{_id} - Socket has been shutdown. Closing connection.");
                    _socket?.Close();
                    break;

                case SocketError.InvalidArgument:
                    NLog.Error($"{_id} - Invalid argument passed to socket. Closing connection.");
                    _socket?.Close();
                    break;

                case SocketError.ConnectionReset:
                case SocketError.NotConnected:
                case SocketError.Disconnecting:
                    NLog.Error($"{_id} - Connection reset or disconnected. Closing socket.");
                    _socket?.Close();
                    break;

                case SocketError.HostUnreachable:
                case SocketError.NetworkDown:
                case SocketError.NetworkUnreachable:
                    NLog.Error($"{_id} - Network unreachable or down. Closing connection.");
                    _socket?.Close();
                    break;

                default:
                    NLog.Error($"{_id} - Unknown socket error. Closing connection.");
                    _socket?.Close();
                    break;
            }
        }
    }
}