using System.Collections.Concurrent;
using NETServer.Infrastructure.Interfaces;

namespace NETServer.Infrastructure.Security
{
    /// <summary>
    /// Class xử lý giới hạn số lượng yêu cầu của mỗi địa chỉ IP trong một khoảng thời gian nhất định.
    /// </summary>
    internal class RequestLimiter : IRequestLimiter
    {
        private readonly (int MaxRequests, TimeSpan TimeWindow) _requestLimit;
        private readonly int _lockoutDuration;
        private readonly ConcurrentDictionary<string, DateTime> _blockedIps = new();
        private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _userRequests = new();

        /// <summary>
        /// Khởi tạo đối tượng RequestLimiter với các tham số cấu hình.
        /// </summary>
        /// <param name="requestLimit">Giới hạn số lượng yêu cầu và thời gian cửa sổ yêu cầu.</param>
        /// <param name="lockoutDuration">Thời gian khóa địa chỉ IP sau khi vượt quá giới hạn yêu cầu.</param>
        public RequestLimiter((int MaxRequests, TimeSpan TimeWindow) requestLimit, int lockoutDuration = 300)
        {
            _requestLimit = requestLimit;
            _lockoutDuration = lockoutDuration;
        }

        /// <summary>
        /// Kiểm tra xem địa chỉ IP có được phép gửi yêu cầu hay không, dựa trên số lượng yêu cầu và tình trạng khóa.
        /// </summary>
        /// <param name="ipAddress">Địa chỉ IP cần kiểm tra.</param>
        /// <returns>True nếu yêu cầu được phép, False nếu không.</returns>
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
            while (requests.TryPeek(out DateTime requestTime) &&
                (currentTime - requestTime).TotalSeconds >= _requestLimit.TimeWindow.TotalSeconds)
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

        /// <summary>
        /// Phương thức làm sạch các IP không còn yêu cầu trong danh sách.
        /// </summary>
        /// <param name="cancellationToken">Token hủy để kiểm soát việc hủy bỏ phương thức bất đồng bộ.</param>
        /// <returns>Task đại diện cho công việc làm sạch yêu cầu bất đồng bộ.</returns>
        public async Task ClearInactiveRequests(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
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
                await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
            }
        }

        /// <summary>
        /// Phương thức làm sạch các IP bị khóa sau khi hết thời gian khóa.
        /// </summary>
        public void ClearBlockedIps()
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
        }

        /// <summary>
        /// Phương thức làm sạch các IP bị khóa sau khi hết thời gian khóa, thực hiện định kỳ.
        /// </summary>
        /// <param name="timeSecond">Số giây giữa mỗi lần làm sạch.</param>
        /// <param name="cancellationToken">Token hủy để kiểm soát việc hủy bỏ phương thức bất đồng bộ.</param>
        /// <returns>Task đại diện cho công việc làm sạch IP bị khóa định kỳ.</returns>
        public async Task ClearBlockedIpsPeriodically(int timesecond, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
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

                // Điều chỉnh thời gian làm sạch, ví dụ mỗi timesecond giây
                await Task.Delay(TimeSpan.FromSeconds(timesecond), cancellationToken);
            }
        }
    }
}
