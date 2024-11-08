using System.Collections.Concurrent;

namespace NETServer.Application.Security;

internal class RequestLimiter
{
    private readonly int _limit;
    private readonly int _timeWindow;
    private readonly int _lockoutDuration;
    private readonly SemaphoreSlim _lock;
    private readonly ConcurrentDictionary<string, DateTime> _blockedIps;
    private readonly ConcurrentDictionary<string, List<DateTime>> _userRequests;

    public RequestLimiter(int limit, int timeWindow, int lockoutDuration = 300)
    {
        _limit = limit;
        _timeWindow = timeWindow;
        _lockoutDuration = lockoutDuration;
        _lock = new SemaphoreSlim(1, 1);
        _blockedIps = new ConcurrentDictionary<string, DateTime>();
        _userRequests = new ConcurrentDictionary<string, List<DateTime>>();
    }

    public async Task<bool> IsAllowed(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            throw new ArgumentException("IP address must be a valid string.");

        DateTime currentTime = DateTime.UtcNow;

        await _lock.WaitAsync();
        try
        {
            // Kiểm tra nếu IP đang bị khóa
            if (_blockedIps.TryGetValue(ipAddress, out DateTime blockEndTime) && currentTime < blockEndTime)
            {
                return false; // IP vẫn bị khóa
            }

            // Xóa IP khỏi danh sách khóa nếu hết thời gian khóa
            _blockedIps.TryRemove(ipAddress, out _);

            // Xóa các yêu cầu cũ hơn _timeWindow
            if (_userRequests.TryGetValue(ipAddress, out var requests) && requests != null)
            {
                requests.RemoveAll(requestTime => (currentTime - requestTime).TotalSeconds >= _timeWindow);
            }
            else
            {
                requests = new List<DateTime>();
                _userRequests[ipAddress] = requests;
            }

            // Kiểm tra số lượng yêu cầu và cập nhật nếu dưới giới hạn
            if (requests.Count < _limit)
            {
                requests.Add(currentTime);
                return true;
            }

            // Khóa IP khi vượt quá giới hạn yêu cầu
            _blockedIps[ipAddress] = currentTime.AddSeconds(_lockoutDuration);
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    // Phương thức làm sạch các IP không còn yêu cầu trong danh sách
    public async Task ClearInactiveRequests()
    {
        while (true)
        {
            DateTime now = DateTime.Now;

            var inactiveIps = _userRequests
                .Where(pair => pair.Value != null && pair.Value.All(t => (now - t).TotalSeconds >= _timeWindow))
                .Select(pair => pair.Key)
                .ToList();

            foreach (var ip in inactiveIps)
            {
                _userRequests.TryRemove(ip, out _);
            }

            await Task.Delay(TimeSpan.FromSeconds(33));
        }
    }
}
