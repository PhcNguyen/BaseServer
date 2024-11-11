using System.Text.RegularExpressions;

namespace NETServer.Logging.Internal;

internal class NLogString
{
    internal static bool FindKeyword(string message, int requiredKeywordCount, HashSet<string> keywords)
    {
        if (string.IsNullOrEmpty(message) || keywords == null || keywords.Count == 0 || requiredKeywordCount <= 0)
            return false;

        int matchCount = 0;

        // Tách các từ trong message sử dụng Regex để loại bỏ dấu câu và khoảng trắng
        var words = Regex.Split(message, @"\W+");

        // Kiểm tra sự xuất hiện của các từ khóa trong HashSet
        foreach (var word in words)
        {
            // Kiểm tra nếu từ xuất hiện trong bộ từ khóa
            if (keywords.Contains(word, StringComparer.OrdinalIgnoreCase))
            {
                matchCount++;
                if (matchCount >= requiredKeywordCount) return true;  // Dừng ngay khi tìm đủ số từ khóa cần thiết
            }
        }

        return false;  // Nếu không tìm đủ số từ khóa yêu cầu
    }

    /*
     * Args:   09:08:45 - INFO - ... 
     * Return: INFO - ...
     */
    internal static string RemoveTimestamp(string log)
    {
        if (log == null) return string.Empty;
        int dashIndex = log.IndexOf(" - ");
        if (dashIndex == -1 || dashIndex < 8) return log;
        return log.Substring(dashIndex + 3);
    }

    /* Định dạng: "HH:mm:ss"
     * Args:   09:08:45 - INFO - ... 
     * Return: 09:08:45
     */
    internal static DateTime ExtractTimestamp(string log) =>
    string.IsNullOrEmpty(log)
        ? throw new ArgumentException("Invalid log format.")
        : DateTime.ParseExact(log.Substring(0, 8), "HH:mm:ss", null);

    /*
     * Args:   09:08:45 - INFO - Session e2b357fd-c752-4a26-8f19-0629f1fff9a6 disconnected from 192.168.1.8:25383
     * Return: 09:08:45 - INFO - 192.168.1.8:25383 disconnected
     */
    internal static string ExtractSession(string log)
    {
        if (string.IsNullOrEmpty(log)) return log;

        var pattern = @"^(\d{2}:\d{2}:\d{2})\s*-\s*(\w+)\s*-\s*Session\s*[a-f0-9-]+\s*(connected|disconnected)\s*(?:from\s*)?(\d+\.\d+\.\d+\.\d+:\d+)";

        var match = Regex.Match(log, pattern);

        if (match.Success)
        {
            // Trích xuất các nhóm thông tin
            string time = match.Groups[1].Value;
            string logLevel = match.Groups[2].Value.ToUpper();
            string status = match.Groups[3].Value;
            string ipPort = match.Groups[4].Value;

            // Trả về kết quả với định dạng mong muốn
            return $"{time} - {logLevel} - {ipPort} {status}";
        }

        return log;
    }

}


internal class NLogProcessor
{
    public ProcessedMessage ProcessLogMessage(string message, NLogLevel level)
    {
        var messageNew = NLogFormatter.FormatTimestamp(message);

        return new ProcessedMessage
        {
            ConsoleMessage = messageNew[0],
            FileMessage = messageNew[1]
        };
    }
}

internal class ProcessedMessage
{
    public string ConsoleMessage { get; set; } = string.Empty;
    public string FileMessage { get; set; } = string.Empty;
}
