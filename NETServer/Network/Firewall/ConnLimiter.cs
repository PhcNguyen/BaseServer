using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using NETServer.Infrastructure.Interfaces;

namespace NETServer.Network.Firewall
{
    /// <summary>
    /// Lớp xử lý giới hạn số lượng kết nối đồng thời từ mỗi địa chỉ IP.
    /// </summary>
    /// <remarks>
    /// Khởi tạo đối tượng ConnectionLimiter với số lượng kết nối tối đa mỗi IP có thể mở.
    /// </remarks>
    /// <param name="maxConnectionsPerIp">Số lượng kết nối tối đa cho mỗi địa chỉ IP.</param>
    internal class ConnLimiter(int maxConnectionsPerIp) : IConnLimiter
    {
        private readonly int _maxConnectionsPerIp = maxConnectionsPerIp;
        private readonly ConcurrentDictionary<string, int> _ipConnectionCounts = new();

        /// <summary>
        /// Kiểm tra xem kết nối từ địa chỉ IP có được phép hay không, dựa trên số lượng kết nối hiện tại.
        /// </summary>
        /// <param name="ipAddress">Địa chỉ IP cần kiểm tra.</param>
        /// <returns>True nếu kết nối được phép, False nếu không.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsConnectionAllowed(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress)) return false;

            // Sử dụng phương thức AddOrUpdate không khóa để cập nhật và kiểm tra số lượng kết nối
            int newConnectionCount = _ipConnectionCounts.AddOrUpdate(ipAddress, 1, (key, oldValue) => oldValue + 1);

            // Kiểm tra sau khi tăng số lượng kết nối
            return newConnectionCount <= _maxConnectionsPerIp;
        }

        /// <summary>
        /// Phương thức gọi khi kết nối bị đóng từ một địa chỉ IP.
        /// </summary>
        /// <param name="ipAddress">Địa chỉ IP cần cập nhật sau khi kết nối đóng.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConnectionClosed(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress)) return false;

            // Nếu IP đang có kết nối, giảm số lượng kết nối đi 1
            if (_ipConnectionCounts.TryGetValue(ipAddress, out int currentCount) && currentCount > 0)
            {
                int newCount = currentCount - 1;

                if (newCount == 0)
                {
                    // Nếu số lượng kết nối bằng 0, xóa IP khỏi danh sách
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