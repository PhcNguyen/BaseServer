using System;

namespace Base.Infrastructure.Configuration
{
    internal static class NetworkConfig
    {
        // Địa chỉ IP của server, null cho phép lắng nghe trên tất cả các địa chỉ IP có sẵn
        public readonly static string? IPAddress = null;

        // Cổng mà server sẽ lắng nghe kết nối (mặc định là 65000)
        public readonly static int Port = 65000;

        // Giới hạn số kết nối tối đa đồng thời mà server có thể xử lý
        public readonly static int MaxConnections = 2000;

        // Độ trễ (tính bằng millisecond) giữa các yêu cầu từ client đến server
        public readonly static int RequestDelayMilliseconds = 50;

        // Thời gian phiên làm việc của client trước khi hết hạn (5s)
        public readonly static TimeSpan ClientSessionTimeout = TimeSpan.FromSeconds(20);

        // Giới hạn yêu cầu tối đa trong một cửa sổ thời gian (ví dụ: 10 yêu cầu trong 0.1 giây)
        public readonly static (int MaxRequests, TimeSpan TimeWindow) RateLimit = (10, TimeSpan.FromSeconds(0.1));

        // Thời gian khóa kết nối khi vượt quá giới hạn yêu cầu (300 giây)
        public readonly static int ConnectionLockoutDuration = 300;

        // Giới hạn số kết nối tối đa từ một địa chỉ IP (ví dụ: 5 kết nối từ cùng một IP)
        public readonly static int MaxConnectionsPerIpAddress = 20;

        // Tốc độ chậm (ví dụ: 512 KB/s, 1 MB/s) - Tốc độ cao (ví dụ: 5 MB/s, 10 MB/s)
        public readonly static int BytesPerSecond = 1048576 / 2; 

        // Chế độ không chặn
        public readonly static bool Blocking = false;

        // Chế độ Keep-Alive cho kết nối
        public readonly static bool KeepAlive = false;

        // Cho phép tái sử dụng địa chỉ
        public readonly static bool ReuseAddress = false;

        // Kích thước hàng đợi kết nối chờ
        public readonly static int QueueSize = 100;

        // Thiết lập bộ đệm gửi 
        public readonly static int SendBuffer = 8192;

        // Thiết lập bộ đệm nhận 
        public readonly static int ReceiveBuffer = 8192;

        // Thiết lập thời gian chờ kết nối 
        public readonly static int SendTimeout = 5000;
        public readonly static int ReceiveTimeout = 5000;
    }
}