using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace NETServer.Application.Security;

internal class ConnectionLimiter
{
    private readonly int _maxConnectionsPerIp;
    private readonly ConcurrentDictionary<string, int> _ipConnectionCounts;
    private readonly SemaphoreSlim _lock;

    public ConnectionLimiter(int maxConnectionsPerIp)
    {
        _maxConnectionsPerIp = maxConnectionsPerIp;
        _ipConnectionCounts = new ConcurrentDictionary<string, int>();
        _lock = new SemaphoreSlim(1, 1); // SemaphoreSlim để đồng bộ truy cập
    }

    // Kiểm tra xem kết nối có được phép hay không
    public async Task<bool> IsConnectionAllowed(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            throw new ArgumentException("IP address cannot be null or empty.");

        await _lock.WaitAsync();
        try
        {
            // Kiểm tra số lượng kết nối hiện tại từ IP
            if (_ipConnectionCounts.TryGetValue(ipAddress, out int currentCount) && currentCount >= _maxConnectionsPerIp)
            {
                return false; // Vượt quá giới hạn kết nối từ IP
            }

            // Tăng số lượng kết nối từ IP
            _ipConnectionCounts.AddOrUpdate(ipAddress, 1, (key, oldValue) => oldValue + 1);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    // Khi kết nối từ IP đóng
    public async Task ConnectionClosed(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            throw new ArgumentException("IP address cannot be null or empty.");

        await _lock.WaitAsync();
        try
        {
            if (_ipConnectionCounts.TryGetValue(ipAddress, out int currentCount) && currentCount > 0)
            {
                // Giảm số lượng kết nối từ IP
                var newCount = currentCount - 1;

                if (newCount == 0)
                {
                    // Xóa khỏi từ điển nếu không còn kết nối
                    _ipConnectionCounts.TryRemove(ipAddress, out _);
                }
                else
                {
                    _ipConnectionCounts[ipAddress] = newCount;
                }
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    // Kiểm tra tất cả các IP đang có kết nối
    public int GetActiveConnections(string ipAddress)
    {
        _ipConnectionCounts.TryGetValue(ipAddress, out int currentCount);
        return currentCount;
    }
}
