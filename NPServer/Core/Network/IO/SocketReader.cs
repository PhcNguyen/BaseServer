using NPServer.Core.Interfaces.Pooling;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Core.Network.IO
{
    /// <summary>
    /// Lớp SocketReader dùng để đọc dữ liệu từ socket một cách không đồng bộ.
    /// </summary>
    public partial class SocketReader : IDisposable
    {
        private readonly Socket _socket;
        private readonly IMultiSizeBufferPool _multiSizeBuffer;
        private readonly SocketAsyncEventArgs _receiveEventArgs;

        private byte[] _buffer;
        private bool _disposed = false;
        private CancellationTokenSource? _cts;

        // Sự kiện khi dữ liệu được nhận đầy đủ
        public event EventHandler<SocketReceivedEventArgs>? DataReceived;

        /// <summary>
        /// Kiểm tra xem đối tượng đã được giải phóng hay chưa.
        /// </summary>
        public bool Disposed => _disposed;

        /// <summary>
        /// Khởi tạo một đối tượng <see cref="SocketReader"/> mới.
        /// </summary>
        /// <param name="socket">Socket dùng để nhận dữ liệu.</param>
        /// <exception cref="ArgumentNullException">Ném ra khi socket là null.</exception>
        public SocketReader(Socket socket, IMultiSizeBufferPool multiSizeBuffer)
        {
            _cts = new CancellationTokenSource();
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _multiSizeBuffer = multiSizeBuffer ?? throw new ArgumentNullException(nameof(multiSizeBuffer));

            _buffer = _multiSizeBuffer.RentBuffer(256);
            _receiveEventArgs = new SocketAsyncEventArgs();
            _receiveEventArgs.SetBuffer(_buffer, 0, _buffer.Length);
            _receiveEventArgs.Completed += OnReceiveCompleted!;
        }

        /// <summary>
        /// Bắt đầu nhận dữ liệu từ socket.
        /// </summary>
        /// <param name="externalCancellationToken">Token hủy bỏ từ bên ngoài (tùy chọn).</param>
        /// <exception cref="ObjectDisposedException">Khi socket đã bị dispose.</exception>
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
        /// <exception cref="ObjectDisposedException">Khi socket đã bị dispose.</exception>
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
                Dispose();
                throw new InvalidOperationException($"Socket disposed: {ex.Message}", ex);
            }
            catch (InvalidOperationException)
            {
                Dispose();
            }
            catch (Exception ex)
            {
                Dispose();
                throw new InvalidOperationException($"Unexpected error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Xử lý sự kiện hoàn thành nhận dữ liệu.
        /// </summary>
        /// <param name="sender">Đối tượng gửi sự kiện.</param>
        /// <param name="e">Thông tin sự kiện.</param>
        /// <exception cref="InvalidOperationException">Khi có lỗi trong quá trình nhận dữ liệu.</exception>
        private async void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (_disposed) return;

                if (!HandleSocketError(e))
                {
                    Dispose();
                    throw new InvalidOperationException($"Socket error: {e.SocketError}");
                }

                int bytesRead = e.BytesTransferred;
                if (bytesRead > 0 && e.Buffer != null && bytesRead >= 4)
                {
                    ReadOnlySpan<byte> sizeBytes = e.Buffer.AsSpan(0, 4);
                    int dataSize = BitConverter.ToInt32(sizeBytes);

                    // Kiểm tra kích thước và điều chỉnh bộ đệm nếu cần
                    ResizeBufferIfNeeded(dataSize);

                    // Tạo sự kiện khi dữ liệu đã đầy đủ
                    OnDataReceived(new SocketReceivedEventArgs(e.Buffer.Take(bytesRead).ToArray()));
                }
                else
                {
                    await Task.Delay(20).ConfigureAwait(false);
                    await Task.Yield();
                }

                // Tiếp tục nhận dữ liệu
                StartReceiving();
            }
            catch (OperationCanceledException)
            {
                Dispose();
            }
            catch (Exception ex)
            {
                Dispose();
                throw new InvalidOperationException($"Error in OnReceiveCompleted: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Phương thức gọi sự kiện khi dữ liệu đã nhận đầy đủ.
        /// </summary>
        /// <param name="e">Thông tin sự kiện.</param>
        protected virtual void OnDataReceived(SocketReceivedEventArgs e)
        {
            DataReceived?.Invoke(this, e);
        }

        /// <summary>
        /// Giải phóng tài nguyên không đồng bộ.
        /// </summary>
        /// <returns>Nhiệm vụ đại diện cho quá trình giải phóng tài nguyên.</returns>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            try
            {
                if (disposing)
                {
                    // Hủy đăng ký sự kiện an toàn
                    if (_receiveEventArgs != null)
                    {
                        _receiveEventArgs.Completed -= OnReceiveCompleted!;
                    }

                    // Giải phóng bộ đệm
                    if (_buffer != null)
                    {
                        _multiSizeBuffer.ReturnBuffer(_buffer);
                        _buffer = [];
                    }

                    // Hủy bỏ cancellation token source
                    _cts?.Cancel();
                    _cts?.Dispose();

                    // Đóng socket an toàn
                    _socket?.Close(timeout: 1000); // Thêm timeout
                }
            }
            finally
            {
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Kiểm tra và điều chỉnh bộ đệm nếu kích thước dữ liệu nhận được lớn hơn kích thước bộ đệm hiện tại.
        /// </summary>
        /// <param name="dataSize">Kích thước dữ liệu nhận được.</param>
        private void ResizeBufferIfNeeded(int dataSize)
        {
            if (dataSize > _buffer.Length)
            {
                _multiSizeBuffer.ReturnBuffer(_buffer);
                _buffer = _multiSizeBuffer.RentBuffer(dataSize);
                _receiveEventArgs.SetBuffer(_buffer, 0, _buffer.Length);
            }
        }

        private readonly Func<SocketAsyncEventArgs, bool> HandleSocketError = (e) =>
        {
            return e.SocketError == SocketError.Success;
        };
    }
}