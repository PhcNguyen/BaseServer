using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using NServer.Interfaces.Core.Network;

namespace NServer.Core.Network.Firewall
{
    /// <summary>
    /// Lớp xử lý giới hạn số lượng kết nối đồng thời từ mỗi địa chỉ IP.
    /// </summary>
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

            // Cập nhật số lượng kết nối của IP trong dictionary
            int newConnectionCount = _ipConnectionCounts.AddOrUpdate(ipAddress, 1, (key, oldValue) => oldValue + 1);

            // Kiểm tra nếu số lượng kết nối vượt quá giới hạn
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

        /// <summary>
        /// Lấy số lượng kết nối hiện tại của một IP.
        /// </summary>
        /// <param name="ipAddress">Địa chỉ IP cần lấy số lượng kết nối.</param>
        /// <returns>Số lượng kết nối hiện tại.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCurrentConnectionCount(string ipAddress)
        {
            return _ipConnectionCounts.GetValueOrDefault(ipAddress, 0);
        }
    }
}