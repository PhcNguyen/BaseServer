using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Base.Core.Network.BufferPool;
using Base.Infrastructure.Services;

namespace Base.Core.Network
{
    /// <summary>
    /// Lớp SocketReader dùng để đọc dữ liệu từ socket một cách không đồng bộ.
    /// </summary>
    internal class SocketReader : IAsyncDisposable, IDisposable
    {
        private readonly Socket _socket;
        private readonly Action<byte[]> _processReceivedData;
        private readonly SocketAsyncEventArgs _receiveEventArgs;
        private readonly MultiSizeBuffer _multiSizeBuffer = Singleton.GetInstance<MultiSizeBuffer>();

        private byte[] _buffer;
        private bool _disposed = false;
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Kiểm tra xem đối tượng đã được giải phóng hay chưa.
        /// </summary>
        public bool Disposed => _disposed;

        /// <summary>
        /// Khởi tạo một đối tượng <see cref="SocketReader"/> mới.
        /// </summary>
        /// <param name="socket">Socket dùng để nhận dữ liệu.</param>
        /// <param name="processReceivedData">Hàm xử lý dữ liệu nhận được.</param>
        /// <exception cref="ArgumentNullException">Ném ra khi socket là null.</exception>
        public SocketReader(Socket socket, Action<byte[]> processReceivedData)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _processReceivedData = processReceivedData;

            _buffer = _multiSizeBuffer.RentBuffer(256);
            _receiveEventArgs = new SocketAsyncEventArgs();
            _receiveEventArgs.SetBuffer(_buffer, 0, _buffer.Length);
            _receiveEventArgs.Completed += OnReceiveCompleted!;

            _cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Bắt đầu nhận dữ liệu từ socket.
        /// </summary>
        /// <param name="externalCancellationToken">Token hủy bỏ từ bên ngoài (tùy chọn).</param>
        public void Receive(CancellationToken? externalCancellationToken = null)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (externalCancellationToken != null)
            {
                // Liên kết token bên ngoài với token nội bộ
                _cts = CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value);
            }

            StartReceiving();
        }

        /// <summary>
        /// Bắt đầu quá trình nhận dữ liệu từ socket.
        /// </summary>
        private void StartReceiving()
        {
            if (_disposed) return;

            try
            {
                if (!_socket.ReceiveAsync(_receiveEventArgs))
                {
                    OnReceiveCompleted(this, _receiveEventArgs);
                }
            }
            catch (ObjectDisposedException ex)
            {
                HandleError($"Socket disposed: {ex.Message}");
            }
            catch (Exception ex)
            {
                HandleError($"Unexpected error in StartReceiving: {ex.Message}");
            }
        }

        /// <summary>
        /// Xử lý sự kiện hoàn thành nhận dữ liệu.
        /// </summary>
        /// <param name="sender">Đối tượng gửi sự kiện.</param>
        /// <param name="e">Thông tin sự kiện.</param>
        private async void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (_disposed) return;

                if (e.SocketError != SocketError.Success)
                {
                    HandleError($"Socket error: {e.SocketError}");
                    await DisposeAsync();
                    return;
                }

                int bytesRead = e.BytesTransferred;
                if (bytesRead > 0 && e.Buffer != null && bytesRead >= 4)
                {
                    byte[] sizeBytes = e.Buffer.Take(4).ToArray();
                    int dataSize = BitConverter.ToInt32(sizeBytes, 0);

                    // Kiểm tra kích thước và điều chỉnh bộ đệm nếu cần
                    if (dataSize > _buffer.Length)
                    {
                        _multiSizeBuffer.ReturnBuffer(_buffer);
                        _buffer = _multiSizeBuffer.RentBuffer(dataSize);
                        _receiveEventArgs.SetBuffer(_buffer, 0, _buffer.Length);
                    }

                    // Gọi phương thức xử lý dữ liệu bất đồng bộ
                    _processReceivedData(e.Buffer.Take(bytesRead).ToArray());
                }

                // Tiếp tục nhận dữ liệu
                StartReceiving();
            }
            catch (OperationCanceledException)
            {
                HandleError("Operation canceled.");
            }
            catch (Exception ex)
            {
                HandleError($"Error in OnReceiveCompleted: {ex.Message}");
            }
            finally
            {
                // Hủy sự kiện sau khi đã xử lý xong
                _receiveEventArgs.Completed -= OnReceiveCompleted!;
            }
        }

        /// <summary>
        /// Xử lý lỗi và ghi thông báo lỗi ra console.
        /// </summary>
        /// <param name="errorMessage">Thông báo lỗi.</param>
        private static void HandleError(string errorMessage)
        {
            Console.WriteLine(errorMessage);
        }

        /// <summary>
        /// Giải phóng tài nguyên không đồng bộ.
        /// </summary>
        /// <returns>Nhiệm vụ đại diện cho quá trình giải phóng tài nguyên.</returns>
        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            _disposed = true;

            _receiveEventArgs.Completed -= OnReceiveCompleted!;

            if (_buffer != null)
            {
                _multiSizeBuffer.ReturnBuffer(_buffer);
                _buffer = [];
            }

            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }

            GC.SuppressFinalize(this);

            await Task.Run(() => _socket.Dispose());
        }

        /// <summary>
        /// Giải phóng tài nguyên đồng bộ.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            _receiveEventArgs.Completed -= OnReceiveCompleted!;

            if (_buffer != null)
            {
                _multiSizeBuffer.ReturnBuffer(_buffer);
                _buffer = [];
            }

            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer để đảm bảo tài nguyên được giải phóng khi đối tượng bị hủy.
        /// </summary>
        ~SocketReader()
        {
            Dispose();
        }
    }
}