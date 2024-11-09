using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NETServer.Application.Security;

internal class RequestLimiter
{
    private readonly int _limit;
    private readonly int _timeWindow;
    private readonly int _lockoutDuration;
    private readonly SemaphoreSlim _lock;
    private readonly ConcurrentDictionary<string, DateTime> _blockedIps;
    private readonly ConcurrentDictionary<string, Queue<DateTime>> _userRequests;

    public RequestLimiter(int limit, int timeWindow, int lockoutDuration = 300)
    {
        _limit = limit;
        _timeWindow = timeWindow;
        _lockoutDuration = lockoutDuration;
        _lock = new SemaphoreSlim(1, 1);
        _blockedIps = new ConcurrentDictionary<string, DateTime>();
        _userRequests = new ConcurrentDictionary<string, Queue<DateTime>>();
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

            // Lấy danh sách các yêu cầu của IP
            if (!_userRequests.ContainsKey(ipAddress))
            {
                _userRequests[ipAddress] = new Queue<DateTime>();
            }

            var requests = _userRequests[ipAddress];

            // Loại bỏ các yêu cầu cũ hơn _timeWindow
            while (requests.Count > 0 && (currentTime - requests.Peek()).TotalSeconds >= _timeWindow)
            {
                requests.Dequeue();
            }

            // Kiểm tra số lượng yêu cầu và cập nhật nếu dưới giới hạn
            if (requests.Count < _limit)
            {
                requests.Enqueue(currentTime);
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
        // Làm sạch định kỳ và loại bỏ các IP không còn yêu cầu
        while (true)
        {
            DateTime now = DateTime.Now;
            List<string> inactiveIps = new List<string>();

            // Kiểm tra và làm sạch các yêu cầu không còn hợp lệ
            foreach (var pair in _userRequests)
            {
                string ip = pair.Key;
                var requests = pair.Value;

                // Nếu tất cả các yêu cầu của IP này đã hết thời gian
                if (requests.All(t => (now - t).TotalSeconds >= _timeWindow))
                {
                    inactiveIps.Add(ip);
                }
            }

            // Xóa các IP không còn hoạt động
            foreach (var ip in inactiveIps)
            {
                _userRequests.TryRemove(ip, out _);
            }

            // Điều chỉnh thời gian làm sạch, ví dụ mỗi 60 giây
            await Task.Delay(TimeSpan.FromSeconds(60));
        }
    }

    // Phương thức làm sạch các IP bị khóa sau khi hết thời gian khóa
    public async Task ClearBlockedIps()
    {
        while (true)
        {
            DateTime now = DateTime.Now;
            List<string> expiredIps = new List<string>();

            // Kiểm tra và xóa các IP hết thời gian khóa
            foreach (var pair in _blockedIps)
            {
                if ((now - pair.Value).TotalSeconds > _lockoutDuration)
                {
                    expiredIps.Add(pair.Key);
                }
            }

            // Xóa các IP bị khóa đã hết thời gian
            foreach (var ip in expiredIps)
            {
                _blockedIps.TryRemove(ip, out _);
            }

            // Điều chỉnh thời gian làm sạch, ví dụ mỗi 60 giây
            await Task.Delay(TimeSpan.FromSeconds(60));
        }
    }
}
