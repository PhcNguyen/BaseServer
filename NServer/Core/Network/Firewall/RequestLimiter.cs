using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Base.Core.Interfaces.Network;

namespace Base.Core.Network.Firewall
{
    /// <summary>
    /// Lớp xử lý giới hạn số lượng yêu cầu từ mỗi địa chỉ IP trong một cửa sổ thời gian.
    /// </summary>
    internal class RequestLimiter((int MaxRequests, TimeSpan TimeWindow) requestLimit, int lockoutDuration = 300) : IRequestLimiter
    {
        private readonly (int MaxRequests, TimeSpan TimeWindow) _requestLimit = requestLimit;
        private readonly int _lockoutDuration = lockoutDuration;
        private readonly ConcurrentDictionary<string, (Queue<DateTime> Requests, DateTime? BlockedUntil)> _ipData = new();

        /// <summary>
        /// Kiểm tra xem một địa chỉ IP có được phép gửi yêu cầu hay không, dựa trên số lượng yêu cầu đã thực hiện.
        /// </summary>
        /// <param name="ipAddress">Địa chỉ IP cần kiểm tra.</param>
        /// <returns>True nếu yêu cầu được phép, False nếu bị giới hạn hoặc bị khóa.</returns>
        public bool IsAllowed(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                throw new ArgumentException("IP address must be a valid string.");

            DateTime currentTime = DateTime.UtcNow;

            // Kiểm tra nếu IP bị khóa và trả về false nếu còn trong thời gian khóa
            if (_ipData.TryGetValue(ipAddress, out var ipInfo) && ipInfo.BlockedUntil.HasValue)
            {
                if (currentTime < ipInfo.BlockedUntil.Value)
                {
                    return false; // IP vẫn bị khóa
                }
                else
                {
                    // Hủy khóa sau khi hết thời gian
                    _ipData[ipAddress] = (ipInfo.Requests, null);
                }
            }

            // Lấy hoặc khởi tạo danh sách yêu cầu của IP nếu không có
            var requests = ipInfo.Requests ?? new Queue<DateTime>();

            // Loại bỏ các yêu cầu đã hết thời gian trong cửa sổ yêu cầu
            while (requests.Count > 0 && (currentTime - requests.Peek()).TotalSeconds > _requestLimit.TimeWindow.TotalSeconds)
            {
                requests.Dequeue(); // Loại bỏ yêu cầu cũ nhất
            }

            // Kiểm tra số lượng yêu cầu và cập nhật nếu dưới giới hạn
            if (requests.Count < _requestLimit.MaxRequests)
            {
                requests.Enqueue(currentTime);  // Thêm yêu cầu mới
                _ipData[ipAddress] = (requests, ipInfo.BlockedUntil); // Cập nhật dữ liệu
                return true;
            }

            // Khóa IP khi vượt quá giới hạn yêu cầu
            _ipData[ipAddress] = (requests, currentTime.AddSeconds(_lockoutDuration));
            return false;
        }

        /// <summary>
        /// Phương thức xóa các yêu cầu không hợp lệ sau một khoảng thời gian.
        /// </summary>
        /// <param name="cancellationToken">Token để hủy tác vụ.</param>
        public async Task ClearInactiveRequests(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken); // Kiểm tra định kỳ mỗi 60 giây

                DateTime currentTime = DateTime.UtcNow;

                // Duyệt qua các IP và loại bỏ các yêu cầu đã hết thời gian trong cửa sổ yêu cầu
                foreach (var ip in _ipData.Keys.ToList())  // Lặp qua khóa IP
                {
                    var ipInfo = _ipData[ip];
                    // Xóa yêu cầu hết hạn trong cửa sổ thời gian
                    while (ipInfo.Requests.Count > 0 && 
                        (currentTime - ipInfo.Requests.Peek()).TotalSeconds > _requestLimit.TimeWindow.TotalSeconds)
                    {
                        ipInfo.Requests.Dequeue();  // Loại bỏ yêu cầu cũ nhất
                    }

                    // Nếu không còn yêu cầu hợp lệ, xóa IP khỏi dictionary
                    if (ipInfo.Requests.Count == 0)
                    {
                        _ipData.TryRemove(ip, out _);
                    }
                    else
                    {
                        _ipData[ip] = ipInfo;  // Cập nhật lại danh sách yêu cầu
                    }
                }
            }
        }
    }
}