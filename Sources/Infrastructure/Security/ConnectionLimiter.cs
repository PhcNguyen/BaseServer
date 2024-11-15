using System.Collections.Concurrent;
using NETServer.Infrastructure.Interfaces;

namespace NETServer.Infrastructure.Security
{
    /// <summary>
    /// Lớp xử lý giới hạn số lượng kết nối đồng thời từ mỗi địa chỉ IP.
    /// </summary>
    internal class ConnectionLimiter : IConnectionLimiter
    {
        private readonly int _maxConnectionsPerIp;
        private readonly ConcurrentDictionary<string, int> _ipConnectionCounts = new();

        /// <summary>
        /// Khởi tạo đối tượng ConnectionLimiter với số lượng kết nối tối đa mỗi IP có thể mở.
        /// </summary>
        /// <param name="maxConnectionsPerIp">Số lượng kết nối tối đa cho mỗi địa chỉ IP.</param>
        public ConnectionLimiter(int maxConnectionsPerIp)
        {
            _maxConnectionsPerIp = maxConnectionsPerIp;
        }

        /// <summary>
        /// Kiểm tra xem kết nối từ địa chỉ IP có được phép hay không, dựa trên số lượng kết nối hiện tại.
        /// </summary>
        /// <param name="ipAddress">Địa chỉ IP cần kiểm tra.</param>
        /// <returns>True nếu kết nối được phép, False nếu không.</returns>
        public bool IsConnectionAllowed(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress)) 
                return false;

            // Sử dụng phương thức không khóa để cập nhật và kiểm tra số lượng kết nối
            _ipConnectionCounts.AddOrUpdate(ipAddress, 1, (key, oldValue) =>
            {
                // Trả về oldValue mà không tăng nếu đã đạt giới hạn
                return oldValue >= _maxConnectionsPerIp ? oldValue : oldValue + 1;
            });

            // Kiểm tra lại sau khi cập nhật
            return _ipConnectionCounts[ipAddress] <= _maxConnectionsPerIp;
        }

        /// <summary>
        /// Phương thức gọi khi kết nối bị đóng từ một địa chỉ IP.
        /// </summary>
        /// <param name="ipAddress">Địa chỉ IP cần cập nhật sau khi kết nối đóng.</param>
        public bool ConnectionClosed(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return false;

            if (_ipConnectionCounts.TryGetValue(ipAddress, out int currentCount) && currentCount > 0)
            {
                int newCount = currentCount - 1;

                if (newCount == 0)
                {
                    _ipConnectionCounts.TryRemove(ipAddress, out _);
                }
                else
                {
                    _ipConnectionCounts[ipAddress] = newCount;
                }
                return true;
            }
            return false;
        }
    }
}
