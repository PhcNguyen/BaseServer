using NPServer.Core.Interfaces.Network;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NPServer.Core.Network.Firewall;

/// <summary>
/// Lớp xử lý giới hạn số lượng kết nối đồng thời từ mỗi địa chỉ IP.
/// </summary>
public class ConnLimiter(int _maxConnectionsPerIpAddress) : IConnLimiter
{
    private readonly int _maxConnectionsPerIp = _maxConnectionsPerIpAddress;
    private readonly ConcurrentDictionary<string, int> _ipConnectionCounts = new();

    /// <summary>
    /// Kiểm tra xem kết nối từ địa chỉ IP có được phép hay không, dựa trên số lượng kết nối hiện tại.
    /// </summary>
    /// <param name="ipAddress">Địa chỉ IP cần kiểm tra.</param>
    /// <returns>True nếu kết nối được phép, False nếu không.</returns>
    public bool IsConnectionAllowed(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress)) return false;
        if (this.GetCurrentConnectionCount(ipAddress) >= _maxConnectionsPerIp) return false;

        // Cập nhật số lượng kết nối của IP trong dictionary
        int newConnectionCount = _ipConnectionCounts.AddOrUpdate(
            ipAddress,
            1,  // Initialize to 1 if the IP is not found
            (key, oldValue) => oldValue + 1  // Increment existing value
        );

        // Kiểm tra nếu số lượng kết nối vượt quá giới hạn
        return newConnectionCount <= _maxConnectionsPerIp;
    }

    /// <summary>
    /// Phương thức gọi khi kết nối bị đóng từ một địa chỉ IP.
    /// </summary>
    /// <param name="ipAddress">Địa chỉ IP cần cập nhật sau khi kết nối đóng.</param>
    public bool ConnectionClosed(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress)) return false;

        // Giảm số kết nối và logging lại
        _ipConnectionCounts.AddOrUpdate(ipAddress, 0, (key, currentCount) =>
        {
            int newCount = currentCount - 1;
            if (newCount == 0)
            {
                return 0;  // Remove the entry
            }

            return newCount;
        });

        return true;
    }

    /// <summary>
    /// Lấy số lượng kết nối hiện tại của một IP.
    /// </summary>
    /// <param name="ipAddress">Địa chỉ IP cần lấy số lượng kết nối.</param>
    /// <returns>Số lượng kết nối hiện tại.</returns>
    public int GetCurrentConnectionCount(string ipAddress) =>
        _ipConnectionCounts.GetValueOrDefault(ipAddress, 0);
}