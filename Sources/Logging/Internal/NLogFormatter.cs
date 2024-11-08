using System.Text;

namespace NETServer.Logging.Internal;

internal class NLogFormatter
{
    public static string FormatMessage(string? message, NLogLevel level, Exception? exception)
    {
        StringBuilder lb = new StringBuilder();

        lb.Append(level.ToString().ToUpperInvariant());

        if (!string.IsNullOrEmpty(message)) lb.Append($" - {message}");
        if (exception != null && level <= NLogLevel.Error) lb.Append(FormatExceptionDetails(exception, level));

        return lb.ToString();
    }

    private static string FormatExceptionDetails(Exception ex, NLogLevel level)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"- Exception: {ex.Message}");
        sb.AppendLine($"- Type: {ex.GetType().FullName}");
        sb.AppendLine($"- Stack Trace: {ex.StackTrace}");
        sb.AppendLine($"- Source: {ex.Source}");

        if ( level == NLogLevel.Critical)
        {
            // Đệ quy để lấy thông tin của các lỗi lồng nhau
            var innerEx = ex.InnerException;
            while (innerEx != null)
            {
                sb.AppendLine("---- Inner Exception ----");
                sb.AppendLine($"- Exception: {innerEx.Message}");
                sb.AppendLine($"- Type: {innerEx.GetType().FullName}");
                sb.AppendLine($"- Stack Trace: {innerEx.StackTrace}");
                sb.AppendLine($"- Source: {innerEx.Source}");
                innerEx = innerEx.InnerException;
            }
        }
        
        return sb.ToString();
    }

    public static List<string> FormatTimestamp(string? message, int type = 0)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var lineNumber = NLogHelper.LineNumber++;
        return new List<string> { $"[{lineNumber:D5}:{timestamp}] - {message}", $"{timestamp} - {message}" };
    }
}
