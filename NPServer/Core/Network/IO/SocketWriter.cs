using NPServer.Core.Interfaces.Pooling;
using System;
using System.Net.Sockets;

namespace NPServer.Core.Network.IO
{
    /// <summary>
    /// Lớp này quản lý việc gửi dữ liệu bất đồng bộ qua socket.
    /// </summary>
    public partial class SocketWriter(Socket socket, IMultiSizeBufferPool multiSizeBuffer) : IDisposable
    {
        private bool _disposed = false;
        private readonly SocketAsyncEventArgs _sendEventArgs = new SocketAsyncEventArgs();
        private readonly Socket _socket = socket ?? throw new ArgumentNullException(nameof(socket));
        private readonly IMultiSizeBufferPool _multiSizeBuffer = multiSizeBuffer ?? throw new ArgumentNullException(nameof(multiSizeBuffer));

        public static void OnCompleted(object? sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                throw new SocketException((int)e.SocketError);
            }
        }

        /// <summary>
        /// Gửi dữ liệu qua socket một cách đồng bộ.
        /// </summary>
        /// <param name="data">Dữ liệu cần gửi.</param>
        /// <returns>Trả về True nếu gửi thành công, ngược lại False.</returns>
        public bool Send(byte[] data)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(SocketWriter));
            ArgumentNullException.ThrowIfNull(data);

            // Thuê bộ đệm để sao chép dữ liệu
            byte[] buffer = _multiSizeBuffer.RentBuffer(data.Length);

            try
            {
                Array.Copy(data, buffer, data.Length);

                _sendEventArgs.SetBuffer(buffer, 0, data.Length);
                _sendEventArgs.Completed += OnCompleted;

                // Gửi dữ liệu không đồng bộ
                if (!_socket.SendAsync(_sendEventArgs))
                {
                    // Kiểm tra kết quả gửi đồng bộ
                    return _sendEventArgs.SocketError == SocketError.Success;
                }

                // Kiểm tra kết quả sau khi gửi không đồng bộ
                return _sendEventArgs.SocketError == SocketError.Success;
            }
            finally
            {
                _multiSizeBuffer.ReturnBuffer(buffer);
            }
        }

        /// <summary>
        /// Giải phóng tài nguyên khi đối tượng không còn sử dụng.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Giải phóng tài nguyên khi đối tượng không còn sử dụng.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                if (disposing)
                {
                    // Giải phóng tài nguyên
                    _sendEventArgs.Dispose();
                    _socket?.Dispose();
                }
            }
            catch
            {
                // Xử lý bất kỳ ngoại lệ nào khi dispose
            }
        }
    }
}