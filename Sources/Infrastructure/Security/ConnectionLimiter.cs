using System.Collections.Concurrent;
using NETServer.Infrastructure.Interfaces;

namespace NETServer.Infrastructure.Security
{
    internal class ConnectionLimiter: IConnectionLimiter
    {
        private readonly int _maxConnectionsPerIp;
        private readonly ConcurrentDictionary<string, int> _ipConnectionCounts;

        public ConnectionLimiter(int maxConnectionsPerIp)
        {
            _maxConnectionsPerIp = maxConnectionsPerIp;
            _ipConnectionCounts = new ConcurrentDictionary<string, int>();
        }

        // Kiểm tra xem kết nối có được phép hay không
        public bool IsConnectionAllowed(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                throw new ArgumentException("IP address cannot be null or empty.");

            // Sử dụng phương thức không khóa
            _ipConnectionCounts.AddOrUpdate(ipAddress, 1, (key, oldValue) =>
            {
                // Trả về oldValue mà không tăng nếu đã đạt giới hạn
                return oldValue >= _maxConnectionsPerIp ? oldValue : oldValue + 1;
            });

            // Kiểm tra lại sau khi cập nhật
            return _ipConnectionCounts[ipAddress] <= _maxConnectionsPerIp;
        }

        // Khi kết nối đóng
        public void ConnectionClosed(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                throw new ArgumentException("IP address cannot be null or empty.");

            if (_ipConnectionCounts.TryGetValue(ipAddress, out int currentCount) && currentCount > 0)
            {
                int newCount = currentCount - 1;

                if (newCount == 0)
                {
                    // Xóa khỏi từ điển nếu không còn kết nối
                    _ipConnectionCounts.TryRemove(ipAddress, out _);
                }
                else
                {
                    // Cập nhật giá trị đã giảm vào từ điển
                    _ipConnectionCounts[ipAddress] = newCount;
                }
            }
        }
    }
}
