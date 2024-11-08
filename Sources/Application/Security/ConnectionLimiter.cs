using System.Collections.Concurrent;

namespace NETServer.Application.Security;

internal class ConnectionLimiter
{
    private readonly int _maxConnectionsPerIp;
    private readonly ConcurrentDictionary<string, int> _ipConnectionCounts;

    public ConnectionLimiter(int maxConnectionsPerIp)
    {
        _maxConnectionsPerIp = maxConnectionsPerIp;
        _ipConnectionCounts = new ConcurrentDictionary<string, int>();
    }

    public bool IsConnectionAllowed(string ipAddress)
    {
        if (_ipConnectionCounts.TryGetValue(ipAddress, out int currentCount) && currentCount >= _maxConnectionsPerIp)
        {
            return false; // Nếu đã vượt quá giới hạn kết nối từ IP
        }

        // Tăng số lượng kết nối từ IP
        _ipConnectionCounts.AddOrUpdate(ipAddress, 1, (key, oldValue) => oldValue + 1);
        return true;
    }

    public void ConnectionClosed(string ipAddress)
    {
        if (_ipConnectionCounts.TryGetValue(ipAddress, out int currentCount) && currentCount > 0)
        {
            _ipConnectionCounts[ipAddress] = Math.Max(0, currentCount - 1);
        }
    }
}
