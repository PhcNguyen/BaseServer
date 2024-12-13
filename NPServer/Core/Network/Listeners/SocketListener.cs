using NPServer.Core.Helpers;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Core.Network.Listeners
{
    /// <summary>
    /// SocketListener là lớp quản lý việc lắng nghe kết nối TCP đến server.
    /// </summary>
    public partial class SocketListener : IDisposable
    {
        private Socket _listenerSocket;
        private readonly int _maxConnections;

        /// <summary>
        /// Kiểm tra xem socket có đang lắng nghe hay không.
        /// </summary>
        public bool IsListening => _listenerSocket?.IsBound == true;

        /// <summary>
        /// Khởi tạo một đối tượng <see cref="SocketListener"/> mới.
        /// </summary>
        public SocketListener(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, int maxConnections)
        {
            _maxConnections = maxConnections;
            _listenerSocket = new Socket(addressFamily, socketType, protocolType)
            {
                NoDelay = true,
                ExclusiveAddressUse = false,
                LingerState = new(false, 0)
            };
            SocketConfiguration.ConfigureSocket(_listenerSocket);
        }

        /// <summary>
        /// Bắt đầu lắng nghe các kết nối đến.
        /// </summary>
        /// <param name="ipAddress">Địa chỉ IP để lắng nghe, nếu null thì sử dụng tất cả địa chỉ.</param>
        /// <param name="port">Cổng để lắng nghe.</param>
        /// <exception cref="InvalidOperationException">Khi socket đã được bind trước đó.</exception>
        public void StartListening(string? ipAddress, int port)
        {
            //if (_listenerSocket.IsBound)
            //    throw new InvalidOperationException("Socket is already bound. StartListening cannot be called multiple times.");

            SocketHelper.BindAndListen(_listenerSocket, ipAddress, port, _maxConnections);
        }

        /// <summary>
        /// Dừng việc lắng nghe và đóng socket.
        /// </summary>
        /// <exception cref="InvalidOperationException">Khi xảy ra lỗi trong quá trình đóng socket.</exception>
        public void StopListening()
        {
            SocketHelper.CloseSocket(_listenerSocket);
        }

        /// <summary>
        /// Đặt lại listener, đóng socket và giải phóng tài nguyên.
        /// </summary>
        public void ResetListener()
        {
            StopListening();  // Đảm bảo socket đã đóng.
            Dispose();        // Giải phóng tài nguyên socket.

            // Tạo socket mới để listener có thể sử dụng lại
            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketConfiguration.ConfigureSocket(_listenerSocket);
        }

        /// <summary>
        /// Chấp nhận kết nối client một cách bất đồng bộ.
        /// </summary>
        /// <param name="token">Token hủy để dừng việc chấp nhận kết nối khi cần thiết.</param>
        /// <returns>Socket của client được chấp nhận.</returns>
        /// <exception cref="ObjectDisposedException">Nếu socket đã bị đóng trong quá trình Accept.</exception>
        /// <exception cref="OperationCanceledException">Nếu thao tác bị hủy bởi token.</exception>
        /// <exception cref="Exception">Nếu có lỗi không mong muốn khác xảy ra.</exception>
        public async Task<Socket?> AcceptClientAsync(CancellationToken token)
        {
            try
            {
                return await _listenerSocket.AcceptAsync(token);
            }
            catch (ObjectDisposedException ex)
            {
                throw new InvalidOperationException("Socket was closed during Accept operation.", ex);
            }
            catch (OperationCanceledException ex)
            {
                throw new InvalidOperationException("AcceptClientAsync was cancelled due to cancellation token.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unexpected error occurred while accepting client.", ex);
            }
        }

        /// <summary>
        /// Giải phóng tài nguyên khi không còn cần thiết.
        /// </summary>
        /// <param name="disposing">Xác định có giải phóng tài nguyên quản lý hay không.</param>
        protected virtual void Dispose(bool disposing)
        {
            _listenerSocket?.Dispose();
        }

        /// <summary>
        /// Giải phóng tài nguyên khi không còn cần thiết.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}