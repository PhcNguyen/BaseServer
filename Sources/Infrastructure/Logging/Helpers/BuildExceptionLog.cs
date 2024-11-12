using System.Text;

namespace NETServer.Infrastructure.Logging.Helper;
    
internal static class BuildExceptionLog
{
    /// <summary>
    /// Định dạng chi tiết của một exception bao gồm loại, stack trace và nguồn lỗi.
    /// </summary>
    public static string FormatExceptionDetails(Exception exception)
    {
        if (exception == null) return "No exception details available.";

        var detailsBuilder = new StringBuilder();

        // Thêm thông tin loại lỗi
        detailsBuilder.Append($" - Type: {ExtractTypeName(exception.GetType().FullName)}");

        // Thêm thông tin Stack Trace ngắn
        detailsBuilder.Append($" - Stack Trace: {ExtractShortStackTrace(exception.StackTrace)}");

        // Thêm thông tin nguồn lỗi
        detailsBuilder.Append($" - Source: {ExtractSourceFileName(exception.Source)}");

        // Thêm thông tin về Inner Exception nếu có
        if (exception.InnerException != null)
        {
            detailsBuilder.Append($" - Inner Exception: {FormatExceptionDetails(exception.InnerException)}");
        }

        detailsBuilder.Append($" - Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

        return detailsBuilder.ToString();
    }

    public static string ExtractShortStackTrace(string? stackTrace)
    {
        if (string.IsNullOrEmpty(stackTrace))
            return "No stack trace available.";

        // Cải tiến để trích xuất phần quan trọng nhất của stack trace
        var lastBackslashPosition = stackTrace.LastIndexOf('\\');
        var lastColonPosition = stackTrace.LastIndexOf(':');

        if (lastBackslashPosition >= 0 && lastColonPosition > lastBackslashPosition)
            return stackTrace.Substring(lastBackslashPosition + 1);

        return stackTrace; // Trả về toàn bộ nếu không tìm thấy thông tin cần thiết
    }

    public static string ExtractSourceFileName(string? source)
    {
        // Nếu nguồn không có sẵn, trả về "Unknown Source"
        if (string.IsNullOrEmpty(source))
            return "Unknown Source";

        var lastBackslashPosition = source.LastIndexOf('\\');
        return lastBackslashPosition >= 0 ? source.Substring(lastBackslashPosition + 1) : source;
    }

    public static string ExtractTypeName(string? fullTypeName)
    {
        // Nếu không có tên loại, trả về "Unknown Type"
        if (string.IsNullOrEmpty(fullTypeName))
            return "Unknown Type";

        var lastDotPosition = fullTypeName.LastIndexOf('.');
        return lastDotPosition >= 0 ? fullTypeName.Substring(lastDotPosition + 1) : fullTypeName;
    }
}
