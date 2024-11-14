namespace NETServer.Application.Helper
{
    public class BandwidthThrottler(int bytesPerSecond)
    {
        private readonly int _bytesPerSecond = bytesPerSecond;
        private long _lastSentTime = DateTime.Now.Ticks;

        public async Task ThrottleSend(int bytesSent)
        {
            // Tính thời gian đã trôi qua kể từ lần gửi cuối
            long elapsedTime = DateTime.Now.Ticks - _lastSentTime;
            long targetTime = (long)(bytesSent * 10000000L / _bytesPerSecond); // Tính thời gian mục tiêu để gửi số byte đó

            if (elapsedTime < targetTime)
            {
                // Nếu gửi quá nhanh, dừng lại một chút để giảm tốc độ
                int delayMilliseconds = (int)((targetTime - elapsedTime) / 10000L); // Chuyển sang milliseconds
                if (delayMilliseconds > 0)
                {
                    await Task.Delay(delayMilliseconds);
                }
            }

            // Cập nhật thời gian cuối cùng gửi dữ liệu
            _lastSentTime = DateTime.Now.Ticks;
        }

        public async Task ThrottleReceive(int bytesReceived)
        {
            // Tính toán và chờ theo tốc độ nhận dữ liệu
            long elapsedTime = DateTime.Now.Ticks - _lastSentTime;
            long targetTime = (long)(bytesReceived * 10000000L / _bytesPerSecond);

            if (elapsedTime < targetTime)
            {
                int delayMilliseconds = (int)((targetTime - elapsedTime) / 10000L);
                if (delayMilliseconds > 0)
                {
                    await Task.Delay(delayMilliseconds);
                }
            }

            _lastSentTime = DateTime.Now.Ticks;
        }

        public async Task ThrottleStream(Stream stream, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await ThrottleSend(bytesRead);
                await stream.WriteAsync(buffer, 0, bytesRead);
            }
        }
    }
}

