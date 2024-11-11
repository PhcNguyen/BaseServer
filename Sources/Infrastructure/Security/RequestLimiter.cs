using System.Collections.Concurrent;
using NETServer.Infrastructure.Interfaces;

namespace NETServer.Infrastructure.Security;

internal class RequestLimiter: IRequestLimiter
{
    private readonly (int MaxRequests, TimeSpan TimeWindow) _requestLimit;
    private readonly int _lockoutDuration;
    private readonly ConcurrentDictionary<string, DateTime> _blockedIps;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _userRequests;

    public RequestLimiter((int MaxRequests, TimeSpan TimeWindow) requestLimit, int lockoutDuration = 300)
    {
        _requestLimit = requestLimit;
        _lockoutDuration = lockoutDuration;
        _blockedIps = new ConcurrentDictionary<string, DateTime>();
        _userRequests = new ConcurrentDictionary<string, ConcurrentQueue<DateTime>>();
    }

    public bool IsAllowed(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            throw new ArgumentException("IP address must be a valid string.");

        DateTime currentTime = DateTime.UtcNow;

        // Kiểm tra nếu IP đang bị khóa
        if (_blockedIps.TryGetValue(ipAddress, out DateTime blockEndTime) && currentTime < blockEndTime)
        {
            return false; // IP vẫn bị khóa
        }

        // Lấy hoặc khởi tạo hàng đợi yêu cầu của IP
        var requests = _userRequests.GetOrAdd(ipAddress, _ => new ConcurrentQueue<DateTime>());

        // Loại bỏ các yêu cầu cũ hơn _timeWindow
        while (requests.TryPeek(out DateTime requestTime) && (currentTime - requestTime).TotalSeconds >= _requestLimit.TimeWindow.TotalSeconds)
        {
            requests.TryDequeue(out _);
        }

        // Kiểm tra số lượng yêu cầu và cập nhật nếu dưới giới hạn
        if (requests.Count < _requestLimit.MaxRequests)
        {
            requests.Enqueue(currentTime);
            return true;
        }

        // Khóa IP khi vượt quá giới hạn yêu cầu
        _blockedIps[ipAddress] = currentTime.AddSeconds(_lockoutDuration);
        return false;
    }

    // Phương thức làm sạch các IP không còn yêu cầu trong danh sách
    public async Task ClearInactiveRequests()
    {
        while (true)
        {
            DateTime now = DateTime.UtcNow;
            var inactiveIps = new List<string>();

            // Kiểm tra và làm sạch các yêu cầu không còn hợp lệ
            foreach (var pair in _userRequests)
            {
                var requests = pair.Value;

                // Nếu tất cả các yêu cầu của IP này đã hết thời gian
                if (requests.IsEmpty || requests.All(t => (now - t).TotalSeconds >= _requestLimit.TimeWindow.TotalSeconds))
                {
                    inactiveIps.Add(pair.Key);
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
            DateTime now = DateTime.UtcNow;
            var expiredIps = new List<string>();

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