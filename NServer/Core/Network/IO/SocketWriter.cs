using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NServer.Core.Network.IO
{
    /// <summary>
    /// Lớp này quản lý việc gửi dữ liệu bất đồng bộ qua socket.
    /// </summary>
    public class SocketWriter(Socket socket) : IDisposable
    {
        private bool _disposed = false;
        private readonly SocketAsyncEventArgs _sendEventArgs = new();
        private readonly Socket _socket = socket ?? throw new ArgumentNullException(nameof(socket));

        /// <summary>
        /// Gửi dữ liệu bất đồng bộ qua socket.
        /// </summary>
        /// <param name="data">Dữ liệu cần gửi qua socket.</param>
        /// <returns>Trả về true nếu gửi thành công, ngược lại trả về false.</returns>
        public async Task<bool> SendAsync(byte[] data)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(data);

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            _sendEventArgs.SetBuffer(data, 0, data.Length);
            _sendEventArgs.Completed += CompletedHandler;

            try
            {
                if (!_socket.SendAsync(_sendEventArgs))
                {
                    HandleSendCompletion(tcs, _sendEventArgs.SocketError);
                }
                else
                {
                    return await tcs.Task;
                }
            }
            finally
            {
                _sendEventArgs.Completed -= CompletedHandler;
            }

            return true;

            void CompletedHandler(object? sender, SocketAsyncEventArgs e) =>
                HandleSendCompletion(tcs, e.SocketError);
        }

        /// <summary>
        /// Xử lý kết quả gửi dữ liệu.
        /// </summary>
        /// <param name="tcs">TaskCompletionSource dùng để hoàn thành tác vụ bất đồng bộ.</param>
        /// <param name="socketError">Lỗi socket sau khi gửi dữ liệu.</param>
        private static void HandleSendCompletion(TaskCompletionSource<bool> tcs, SocketError socketError)
        {
            if (socketError == SocketError.Success)
            {
                tcs.TrySetResult(true);
            }
            else
            {
                tcs.TrySetException(new SocketException((int)socketError));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);  // Violates rule
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
                _sendEventArgs.Dispose();
                _socket.Dispose();
            }
            catch
            {
                // Xử lý bất kỳ ngoại lệ nào khi dispose
            }
        }
    }
}