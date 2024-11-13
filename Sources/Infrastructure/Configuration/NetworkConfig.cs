namespace NETServer.Infrastructure.Configuration
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

        // Thời gian phiên làm việc của client trước khi hết hạn (5 phút)
        public readonly static TimeSpan ClientSessionTimeout = TimeSpan.FromMinutes(0.5);

        // Giới hạn yêu cầu tối đa trong một cửa sổ thời gian (ví dụ: 10 yêu cầu trong 1 giây)
        public readonly static (int MaxRequests, TimeSpan TimeWindow) RateLimit = (10, TimeSpan.FromSeconds(1));

        // Thời gian khóa kết nối khi vượt quá giới hạn yêu cầu (300 giây)
        public readonly static int ConnectionLockoutDuration = 300;

        // Giới hạn số kết nối tối đa từ một địa chỉ IP (ví dụ: 5 kết nối từ cùng một IP)
        public readonly static int MaxConnectionsPerIpAddress = 5;
    }
}