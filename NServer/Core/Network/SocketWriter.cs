using System;
using System.Net.Sockets;
using System.Threading.Tasks;

using NServer.Infrastructure.Services;

namespace NServer.Core.Network
{
    /// <summary>
    /// Lớp này quản lý việc gửi dữ liệu bất đồng bộ qua socket.
    /// </summary>
    internal class SocketWriter(Socket socket) : IAsyncDisposable
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

        /// <summary>
        /// Giải phóng tài nguyên bất đồng bộ khi đối tượng không còn sử dụng.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                await Task.Run(() =>
                {
                    _sendEventArgs.Dispose();
                    _socket.Dispose();
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during async dispose: {ex.Message}");
            }
        }

        /// <summary>
        /// Giải phóng tài nguyên đồng bộ khi đối tượng không còn sử dụng.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                _sendEventArgs.Dispose();
                _socket.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during dispose: {ex.Message}");
            }
        }
    }
}
