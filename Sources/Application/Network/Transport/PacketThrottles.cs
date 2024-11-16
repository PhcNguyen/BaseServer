using System.Diagnostics;

namespace NETServer.Application.Network.Transport
{
    /// <summary>
    /// Lớp giúp điều khiển tốc độ gửi và nhận dữ liệu một cách hợp lý để tránh vượt quá tốc độ cho phép.
    /// </summary>
    public class PacketThrottles
    {
        private int BytesPerSecond;  // Tốc độ giới hạn (bytes mỗi giây)
        private readonly Stopwatch _stopwatch;  // Sử dụng Stopwatch

        /// <summary>
        /// Khởi tạo DataThrottler với tốc độ giới hạn cụ thể.
        /// </summary>
        /// <param name="bytesPerSecond">Tốc độ giới hạn (bytes mỗi giây)</param>
        public PacketThrottles(int bytesPerSecond)
        {
            if (bytesPerSecond <= 0)
                throw new ArgumentException("Rate must be greater than 0.");

            this.BytesPerSecond = bytesPerSecond;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        /// <summary>
        /// Phương thức điều khiển tốc độ gửi và nhận dữ liệu.
        /// </summary>
        private async Task Throttle(int bytesProcessed)
        {
            // Kiểm tra thời gian trôi qua
            long elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;
            long targetTime = (bytesProcessed * 1000L) / BytesPerSecond;

            // Điều khiển thời gian gửi/nhận dữ liệu để không vượt quá tốc độ cho phép
            if (elapsedMilliseconds < targetTime)
            {
                int delayMilliseconds = (int)(targetTime - elapsedMilliseconds);
                if (delayMilliseconds > 0)
                {
                    await Task.Delay(delayMilliseconds).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Điều khiển tốc độ gửi dữ liệu bất đồng bộ.
        /// </summary>
        /// <param name="bytesSent">Số byte đã gửi.</param>
        public async Task ThrottleSend(int bytesSent)
        {
            await Throttle(bytesSent);  // Gọi phương thức chung để điều khiển tốc độ
        }

        /// <summary>
        /// Điều khiển tốc độ nhận dữ liệu bất đồng bộ.
        /// </summary>
        /// <param name="bytesReceived">Số byte đã nhận.</param>
        public async Task ThrottleReceive(int bytesReceived)
        {
            await Throttle(bytesReceived);  // Gọi phương thức chung để điều khiển tốc độ
        }

        /// <summary>
        /// Điều khiển luồng dữ liệu bất đồng bộ với kích thước bộ đệm.
        /// </summary>
        public async Task ThrottleStream(Stream stream, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];  // Tạo bộ đệm có kích thước nhất định
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer).ConfigureAwait(false)) > 0)
            {
                await ThrottleSend(bytesRead);
                await stream.WriteAsync(buffer.AsMemory(0, bytesRead)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Cập nhật tốc độ điều khiển dữ liệu (bytes mỗi giây) trong quá trình chạy.
        /// </summary>
        public void UpdateThrottlingRate(int newRate)
        {
            if (newRate <= 0)
            {
                throw new ArgumentException("Rate must be greater than 0.");
            }
            this.BytesPerSecond = newRate;
        }

        /// <summary>
        /// Điều khiển nhiều luồng dữ liệu bất đồng bộ cùng lúc.
        /// </summary>
        public async Task ThrottleMultipleStreams(List<Stream> streams, int bufferSize)
        {
            var tasks = streams.Select(stream => ThrottleStream(stream, bufferSize)).ToList();
            await Task.WhenAll(tasks).ConfigureAwait(false);  // Đợi tất cả các luồng hoàn thành
        }
    }
}
