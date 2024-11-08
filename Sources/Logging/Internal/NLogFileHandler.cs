using System.Text;


namespace NETServer.Logging.Internal;

internal static class NLogFileHandler
{
    private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

    public static async Task WriteLogAsync(string message, NLogLevel level)
    {
        string filePath = GetFilePath(level);

        // Kiểm tra xem thông điệp có phải là mới không
        if (!await IsMessageNew(filePath, message)) return;

        // Xử lý thông điệp
        var processedMessage = new NLogProcessor().ProcessLogMessage(message, level);

        // In log ra console nếu cần
        if (level <= NLog.ConsoleLogLevel) Console.WriteLine(processedMessage.ConsoleMessage);

        // Ghi log vào file nếu cần
        if (level <= NLog.WriteLogLevel)
        {
            await AppendLogToFile(filePath, processedMessage.FileMessage, level);
        }
    }

    private static string GetFilePath(NLogLevel level) =>
        NLogHelper.LevelFileMapping.GetValueOrDefault(level, NLogHelper.DefaultFilePath);

    private static async Task<bool> IsMessageNew(string filePath, string message)
    {
        if (!File.Exists(filePath)) return true;

        try
        {
            var lastLines = await ReadLastLines(filePath, 5);

            foreach (string lastLine in lastLines)
            {
                if (NLogString.RemoveTimestamp(message) == NLogString.RemoveTimestamp(lastLine))
                {
                    return false;
                }
            }

            return true;
        }
        catch
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
        if (NLogString.FindKeyword(message, 1, new HashSet<string> { "session" }))
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
}
