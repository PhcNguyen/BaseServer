namespace NETServer.Application.Helper
{
    public class DataThrottlerHelper(int bytesPerSecond)
    {
        private int _bytesPerSecond = bytesPerSecond;  // Throttle rate
        private long _lastSentTime = DateTime.Now.Ticks;

        // Throttle sending data asynchronously
        public async Task ThrottleSend(int bytesSent)
        {
            long elapsedTime = DateTime.Now.Ticks - _lastSentTime;
            long targetTime = (long)(bytesSent * 10000000L / _bytesPerSecond);

            if (elapsedTime < targetTime)
            {
                int delayMilliseconds = (int)((targetTime - elapsedTime) / 10000L);
                if (delayMilliseconds > 0)
                {
                    await Task.Delay(delayMilliseconds); // Asynchronous delay
                }
            }

            _lastSentTime = DateTime.Now.Ticks;
        }

        // Throttle receiving data asynchronously
        public async Task ThrottleReceive(int bytesReceived)
        {
            long elapsedTime = DateTime.Now.Ticks - _lastSentTime;
            long targetTime = (long)(bytesReceived * 10000000L / _bytesPerSecond);

            if (elapsedTime < targetTime)
            {
                int delayMilliseconds = (int)((targetTime - elapsedTime) / 10000L);
                if (delayMilliseconds > 0)
                {
                    await Task.Delay(delayMilliseconds); // Asynchronous delay
                }
            }

            _lastSentTime = DateTime.Now.Ticks;
        }

        // Throttle data stream asynchronously with buffer size
        public async Task ThrottleStream(Stream stream, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await ThrottleSend(bytesRead); // Throttle data as it is read
                await stream.WriteAsync(buffer, 0, bytesRead); // Write data to stream asynchronously
            }
        }

        // Dynamically update throttling rate at runtime
        public void UpdateThrottlingRate(int newRate)
        {
            _bytesPerSecond = newRate;
        }

        // Asynchronously handle multiple streams in parallel
        public async Task ThrottleMultipleStreams(List<Stream> streams, int bufferSize)
        {
            var tasks = streams.Select(stream => ThrottleStream(stream, bufferSize)).ToList();
            await Task.WhenAll(tasks); // Wait for all streams to finish processing
        }
    }
}
