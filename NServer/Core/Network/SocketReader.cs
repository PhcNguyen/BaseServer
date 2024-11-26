using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

using NServer.Core.Network.BufferPool;
using NServer.Infrastructure.Services;

namespace NServer.Core.Network
{
    internal class SocketReader : IDisposable
    {
        private readonly Socket _socket;
        private readonly Action<byte[]> _processReceivedData;
        private readonly SocketAsyncEventArgs _receiveEventArgs;
        private readonly MultiSizeBuffer _multiSizeBuffer = Singleton.GetInstance<MultiSizeBuffer>();

        private byte[] _buffer;
        private bool _disposed = false;

        public bool Disposed => _disposed;

        public SocketReader(Socket socket, Action<byte[]> processReceivedData)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _processReceivedData = processReceivedData;

            _buffer = _multiSizeBuffer.RentBuffer(256);
            _receiveEventArgs = new SocketAsyncEventArgs();
            _receiveEventArgs.SetBuffer(_buffer, 0, _buffer.Length);
            _receiveEventArgs.Completed += OnReceiveCompleted!;
        }

        public void Receive() => StartReceiving();

        private void StartReceiving()
        {
            // Nếu đang nhận dữ liệu, không gọi lại
            if (_disposed) return;

            try
            {
                if (!_socket.ReceiveAsync(_receiveEventArgs))
                {
                    // Nếu không phải bất đồng bộ, xử lý ngay
                    OnReceiveCompleted(this, _receiveEventArgs);
                }
            }
            catch (ObjectDisposedException ex)
            {
                HandleError(ex.Message);
            }
            catch (Exception ex)
            {
                HandleError($"Unexpected error in StartReceiving: {ex.Message}");
            }
        }

        private async void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError != SocketError.Success)
                {
                    HandleError($"Socket error: {e.SocketError}");
                    Dispose();
                    return;
                }

                int bytesRead = e.BytesTransferred;
                if (bytesRead > 0 && e.Buffer != null && bytesRead >= 4)
                {
                    byte[] sizeBytes = e.Buffer.Take(4).ToArray();
                    int dataSize = BitConverter.ToInt32(sizeBytes, 0);

                    if (dataSize > _buffer.Length)
                    {
                        // Điều chỉnh kích thước bộ đệm nếu cần thiết
                        _buffer = _multiSizeBuffer.RentBuffer(dataSize);
                        _receiveEventArgs.SetBuffer(_buffer, 0, _buffer.Length);
                    }

                    _processReceivedData(e.Buffer.Take(bytesRead).ToArray());
                }

                // Tiếp tục nhận dữ liệu
                await Task.Yield(); // Nhường quyền điều khiển để tránh đệ quy quá sâu
                StartReceiving(); // Đảm bảo gọi lại StartReceiving
            }
            catch (Exception ex)
            {
                HandleError($"Error in OnReceiveCompleted: {ex.Message}");
            }
        }

        private static void HandleError(string errorMessage)
        {
            Console.WriteLine(errorMessage);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _receiveEventArgs.Completed -= OnReceiveCompleted!;

            if (_buffer != null)
            {
                _multiSizeBuffer.ReturnBuffer(_buffer);
                _buffer = [];
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        ~SocketReader()
        {
            Dispose();
        }
    }
}