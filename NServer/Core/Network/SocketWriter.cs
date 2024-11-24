using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NServer.Core.Network
{
    internal class SocketWriter : IDisposable
    {
        private readonly Socket _socket;
        private readonly SocketAsyncEventArgs _sendEventArgs;

        public SocketWriter(Socket socket)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _sendEventArgs = new SocketAsyncEventArgs();
            _sendEventArgs.Completed += OnSendCompleted!;
        }

        public async Task WriteAsync(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);

            // Đặt bộ đệm cho dữ liệu sẽ gửi
            _sendEventArgs.SetBuffer(data, 0, data.Length);

            // Gửi dữ liệu bất đồng bộ và đợi kết quả
            var sendResult = await SendAsync(_socket, _sendEventArgs);

            if (sendResult)
            {
                Console.WriteLine("Data sent successfully.");
            }
            else
            {
                Console.WriteLine($"Socket error: {_sendEventArgs.SocketError}");
            }
        }

        private async Task<bool> SendAsync(Socket socket, SocketAsyncEventArgs e)
        {
            var tcs = new TaskCompletionSource<bool>();

            // Đăng ký sự kiện Completed để gọi lại khi gửi xong
            void completedHandler(object sender, SocketAsyncEventArgs args)
            {
                // Hủy đăng ký sự kiện sau khi hoàn thành
                e.Completed -= completedHandler!;
                tcs.SetResult(args.SocketError == SocketError.Success);
            }

            e.Completed += completedHandler!;

            // Gửi dữ liệu bất đồng bộ
            if (!socket.SendAsync(e))
            {
                // Nếu không phải bất đồng bộ, gọi ngay kết quả
                tcs.SetResult(e.SocketError == SocketError.Success);
            }

            return await tcs.Task;
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            // Xử lý lỗi nếu có
            if (e.SocketError != SocketError.Success)
            {
                Console.WriteLine($"Socket error: {e.SocketError}");
            }
        }

        public void Dispose()
        {
            // Giải phóng tài nguyên và hủy đăng ký sự kiện
            _sendEventArgs.Completed -= OnSendCompleted!;
            _sendEventArgs.Dispose();
        }
    }
}