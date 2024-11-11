using System.Text;

namespace NETServer.Logging.Internal
{
    internal static class NLogFileHandler
    {
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
        private static bool _isPaused = false;
        private static StringBuilder _logBuffer = new StringBuilder();

        public static async Task WriteLogAsync(string message, NLogLevel level)
        {
            string filePath = GetFilePath(level);

            // Kiểm tra xem thông điệp có phải là mới không
            if (level != NLogLevel.Info) if (!await IsMessageNew(filePath, message)) return;

            // Xử lý thông điệp
            var processedMessage = new NLogProcessor().ProcessLogMessage(message, level);

            // Ghi log vào file bất kể trạng thái tạm dừng
            if (level <= NLog.WriteLogLevel)
            {
                await AppendLogToFile(filePath, processedMessage.FileMessage, level);
            }

            // Nếu đang tạm dừng, lưu thông điệp vào bộ đệm và không in ra màn hình
            if (_isPaused)
            {
                _logBuffer.AppendLine(processedMessage.ConsoleMessage);
                return;
            }

            // In log ra console nếu cần
            if (level <= NLog.ConsoleLogLevel)
            {
                Console.WriteLine(processedMessage.ConsoleMessage);
            }
        }

        private static string GetFilePath(NLogLevel level) =>
            NLogHelper.LevelFileMapping.GetValueOrDefault(level, NLogHelper.DefaultFilePath);

        private static async Task<bool> IsMessageNew(string filePath, string message)
        {
            if (!File.Exists(filePath)) return true;

            try
            {
                var messageTimestamp = NLogString.ExtractTimestamp(message);
                var lastLines = await ReadLastLines(filePath, 10);

                foreach (string lastLine in lastLines)
                {
                    var lastLineTimestamp = NLogString.ExtractTimestamp(lastLine);
                    var timeDifference = messageTimestamp - lastLineTimestamp;

                    if (NLogString.RemoveTimestamp(message) == NLogString.RemoveTimestamp(lastLine))
                    {
                        if (Math.Abs(timeDifference.TotalSeconds) < 5)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private static async Task<List<string>> ReadLastLines(string filePath, int lineCount)
        {
            var lastLines = new List<string>();

            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lastLines.Add(line);
                    if (lastLines.Count > lineCount)
                    {
                        lastLines.RemoveAt(0);
                    }
                }
            }

            return lastLines;
        }

        private static async Task AppendLogToFile(string filePath, string? message, NLogLevel level)
        {
            if (message == null || filePath == null) return;
            if (NLogString.FindKeyword(message, 2, new HashSet<string> { "Session", "connected", "disconnected" }))
            {
                message = NLogString.ExtractSession(message);
            }

            try
            {
                await Semaphore.WaitAsync();

                if (!File.Exists(filePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                }

                using (var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    await writer.WriteLineAsync(message);
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        // Hàm tạm dừng việc in log ra màn hình
        public static void PauseNLog() => _isPaused = true;

        // Hàm tiếp tục việc in log ra màn hình từ vị trí đã tạm dừng
        public static void ResumeNLog()
        {
            _isPaused = false;

            // In tất cả các thông điệp đã lưu trong bộ đệm
            if (_logBuffer.Length > 0)
            {
                Console.WriteLine(_logBuffer.ToString());
                _logBuffer.Clear(); // Xóa bộ đệm sau khi đã in xong
            }
        }
    }
}
