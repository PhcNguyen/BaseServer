using System.Diagnostics;

namespace NETServer.Application.Helper
{
    /// <summary>
    /// Lớp giúp điều khiển tốc độ gửi và nhận dữ liệu một cách hợp lý để tránh vượt quá tốc độ cho phép.
    /// </summary>
    public class DataThrottlerHelper
    {
        private int _bytesPerSecond;  // Tốc độ giới hạn (bytes mỗi giây)
        private readonly Stopwatch _stopwatch; // Sử dụng Stopwatch

        /// <summary>
        /// Khởi tạo DataThrottlerHelper với tốc độ giới hạn cụ thể.
        /// </summary>
        /// <param name="bytesPerSecond">Tốc độ giới hạn (bytes mỗi giây)</param>
        public DataThrottlerHelper(int bytesPerSecond)
        {
            _bytesPerSecond = bytesPerSecond;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        /// <summary>
        /// Phương thức giúp điều khiển tốc độ gửi dữ liệu bất đồng bộ.
        /// </summary>
        /// <param name="bytesSent">Số byte đã gửi.</param>
        public async Task ThrottleSend(int bytesSent)
        {
            await Throttle(bytesSent);  // Gọi phương thức chung để điều khiển tốc độ
        }

        /// <summary>
        /// Phương thức giúp điều khiển tốc độ nhận dữ liệu bất đồng bộ.
        /// </summary>
        /// <param name="bytesReceived">Số byte đã nhận.</param>
        public async Task ThrottleReceive(int bytesReceived)
        {
            await Throttle(bytesReceived);  // Gọi phương thức chung để điều khiển tốc độ
        }

        /// <summary>
        /// Phương thức chung giúp điều khiển tốc độ gửi và nhận dữ liệu.
        /// </summary>
        /// <param name="bytesProcessed">Số byte đã xử lý (gửi hoặc nhận).</param>
        private async Task Throttle(int bytesProcessed)
        {
            // Tính toán thời gian đã trôi qua kể từ lần gửi/nhận cuối cùng
            long elapsedTime = _stopwatch.ElapsedMilliseconds;
            long targetTime = (bytesProcessed * 1000L) / _bytesPerSecond;

            // Nếu thời gian đã trôi qua chưa đủ, chờ thêm thời gian cần thiết
            if (elapsedTime < targetTime)
            {
                int delayMilliseconds = (int)(targetTime - elapsedTime);
                if (delayMilliseconds > 0)
                {
                    await Task.Delay(delayMilliseconds).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Điều khiển luồng dữ liệu bất đồng bộ với kích thước bộ đệm.
        /// </summary>
        /// <param name="stream">Luồng dữ liệu cần điều khiển.</param>
        /// <param name="bufferSize">Kích thước bộ đệm dùng cho việc đọc và ghi dữ liệu.</param>
        public async Task ThrottleStream(Stream stream, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];  // Tạo bộ đệm có kích thước nhất định
            int bytesRead;

            // Đọc dữ liệu từ stream và điều khiển tốc độ gửi
            while ((bytesRead = await stream.ReadAsync(buffer).ConfigureAwait(false)) > 0)
            {
                await ThrottleSend(bytesRead); 
                await stream.WriteAsync(buffer.AsMemory(0, bytesRead)).ConfigureAwait(false);  
            }
        }

        /// <summary>
        /// Cập nhật tốc độ điều khiển dữ liệu (bytes mỗi giây) trong quá trình chạy.
        /// </summary>
        /// <param name="newRate">Tốc độ mới (bytes mỗi giây).</param>
        public void UpdateThrottlingRate(int newRate)
        {
            if (newRate <= 0)
            {
                throw new ArgumentException("Rate must be greater than 0.");
            }
            _bytesPerSecond = newRate;
        }

        /// <summary>
        /// Điều khiển nhiều luồng dữ liệu bất đồng bộ cùng lúc.
        /// </summary>
        /// <param name="streams">Danh sách các luồng dữ liệu cần điều khiển.</param>
        /// <param name="bufferSize">Kích thước bộ đệm dùng cho việc đọc và ghi dữ liệu.</param>
        public async Task ThrottleMultipleStreams(List<Stream> streams, int bufferSize)
        {
            var tasks = streams.Select(stream => ThrottleStream(stream, bufferSize)).ToList();
            await Task.WhenAll(tasks).ConfigureAwait(false);  // Đợi tất cả các luồng hoàn thành
        }
    }
}
