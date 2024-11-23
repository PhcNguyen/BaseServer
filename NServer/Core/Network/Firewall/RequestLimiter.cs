﻿using NServer.Interfaces.Core.Network;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace NServer.Core.Network.Firewall
{
    /// <summary>
    /// Lớp xử lý giới hạn số lượng yêu cầu từ mỗi địa chỉ IP trong một cửa sổ thời gian.
    /// </summary>
    internal class RequestLimiter((int MaxRequests, TimeSpan TimeWindow) requestLimit, int lockoutDuration = 300) : IRequestLimiter
    {
        private readonly (int MaxRequests, TimeSpan TimeWindow) _requestLimit = requestLimit;
        private readonly int _lockoutDuration = lockoutDuration;
        private readonly ConcurrentDictionary<string, (List<DateTime> Requests, DateTime? BlockedUntil)> _ipData = new();

        /// <summary>
        /// Kiểm tra xem một địa chỉ IP có được phép gửi yêu cầu hay không, dựa trên số lượng yêu cầu đã thực hiện.
        /// </summary>
        /// <param name="ipAddress">Địa chỉ IP cần kiểm tra.</param>
        /// <returns>True nếu yêu cầu được phép, False nếu bị giới hạn hoặc bị khóa.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAllowed(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                throw new ArgumentException("IP address must be a valid string.");

            DateTime currentTime = DateTime.UtcNow;

            // Kiểm tra nếu IP bị khóa
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

            // Lấy hoặc khởi tạo danh sách yêu cầu của IP
            var requests = ipInfo.Requests ?? [];

            // Loại bỏ các yêu cầu đã hết thời gian trong cửa sổ yêu cầu
            requests.RemoveAll(t => (currentTime - t).TotalSeconds > _requestLimit.TimeWindow.TotalSeconds);

            // Kiểm tra số lượng yêu cầu và cập nhật nếu dưới giới hạn
            if (requests.Count < _requestLimit.MaxRequests)
            {
                requests.Add(currentTime);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                    ipInfo.Requests.RemoveAll(t => (currentTime - t).TotalSeconds > _requestLimit.TimeWindow.TotalSeconds);

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