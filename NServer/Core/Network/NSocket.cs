using NServer.Core.Network.Buffers;
using NServer.Infrastructure.Services;

using System.Net.Sockets;
using NServer.Core.Packet;
using NServer.Core.Packet.Utils;

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

        private readonly SocketAsyncEventArgs _receiveEventArgs = new();
        private readonly SocketAsyncEventArgs _sendEventArgs = new();

        public NSocket(Socket socket, Guid id)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _id = id;

            _receiveEventArgs.Completed += OnReceiveCompleted;
            _sendEventArgs.Completed += OnSendCompleted;
        }

        public void StartReceiving() => ReceiveData();

        public void SendData(byte[] data) => SendDataInternal(data);

        public void SendData(Packets packet) => SendData(packet.ToByteArray());

        private void ReceiveData()
        {
            if (_socket == null) return;

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

            ReceiveData(); // Tiếp tục nhận dữ liệu
        }

        private void SendDataInternal(byte[] data)
        {
            if (_socket == null) return;

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

            Console.WriteLine("Data sent successfully.");
        }

        private void ProcessPacket(Guid sessionId, byte[] data)
        {
            if (data.Length < 8) return;

            Packets packet = PacketExtensions.FromByteArray(data);
            if (packet != null)
            {
                _packetContainer.AddPacket(sessionId, packet);
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
            switch (socketError)
            {
                case SocketError.OperationAborted:
                case SocketError.Interrupted:
                    // Lỗi này có thể không phải là vấn đề nghiêm trọng và có thể tiếp tục.
                    Console.WriteLine("Operation aborted or interrupted. Continuing...");
                    break;

                case SocketError.MessageSize:
                    // Lỗi kích thước gói tin quá lớn hoặc không hợp lệ. Bạn có thể thử lại.
                    Console.WriteLine("Message size error. Trying again...");
                    ReceiveData(); // Tiếp tục nhận dữ liệu
                    break;

                case SocketError.TimedOut:
                    // Lỗi timeout, có thể thử lại hoặc tiếp tục nhận.
                    Console.WriteLine("Socket timeout. Attempting to reconnect or continue...");
                    ReceiveData(); // Tiếp tục nhận dữ liệu nếu thời gian chờ hết hạn
                    break;

                case SocketError.ConnectionReset:
                case SocketError.NotConnected:
                case SocketError.Disconnecting:
                    // Lỗi nghiêm trọng liên quan đến kết nối, đóng socket.
                    Console.WriteLine("Connection reset or disconnected. Closing socket.");
                    _socket?.Close();
                    break;

                case SocketError.HostUnreachable:
                case SocketError.NetworkDown:
                case SocketError.NetworkUnreachable:
                    // Các lỗi liên quan đến mạng không thể truy cập hoặc không có mạng
                    Console.WriteLine("Network unreachable or down. Closing connection.");
                    _socket?.Close();
                    break;

                case SocketError.AddressNotAvailable:
                    // Địa chỉ không khả dụng, đóng kết nối
                    Console.WriteLine("Address not available. Closing connection.");
                    _socket?.Close();
                    break;

                case SocketError.ConnectionRefused:
                    // Máy chủ từ chối kết nối, có thể thử lại hoặc thông báo cho người dùng
                    Console.WriteLine("Connection refused by server. Closing connection.");
                    _socket?.Close();
                    break;

                case SocketError.Shutdown:
                    // Kết nối đã đóng, không thể tiếp tục.
                    Console.WriteLine("Socket has been shutdown. Closing connection.");
                    _socket?.Close();
                    break;

                case SocketError.InvalidArgument:
                    // Tham số không hợp lệ, cần điều tra thêm
                    Console.WriteLine("Invalid argument passed to socket. Closing connection.");
                    _socket?.Close();
                    break;

                default:
                    // Các lỗi khác, có thể log chi tiết và đóng socket
                    Console.WriteLine("Unknown socket error. Closing connection.");
                    _socket?.Close();
                    break;
            }
        }
    }
}