using System.Text;

namespace NServer.Infrastructure.Logging.Filter
{
    internal static class FilterException
    {
        /// <summary>
        /// Trích xuất stack trace ngắn với các thông tin quan trọng.
        /// </summary>
        public static string ExtractShortStackTrace(string? stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace))
                return "UNKNOWN";

            // Trích xuất thông tin từ phần cuối của stack trace
            return ExtractLastRelevantSegment(stackTrace, '\\', ':');
        }

        /// <summary>
        /// Trích xuất tên file nguồn (source) của ngoại lệ.
        /// </summary>
        public static string ExtractSourceFileName(string? source)
        {
            return string.IsNullOrEmpty(source) ? "UNKNOWN" : ExtractLastSegment(source, '\\');
        }

        /// <summary>
        /// Trích xuất tên kiểu (type) từ full type name của ngoại lệ.
        /// </summary>
        public static string ExtractTypeName(string? fullTypeName)
        {
            return string.IsNullOrEmpty(fullTypeName) ? "UNKNOWN" : ExtractLastSegment(fullTypeName, '.');
        }

        /// <summary>
        /// Trích xuất thông tin ngắn gọn từ InnerException.
        /// </summary>
        public static string ExtractInnerExceptionDetails(Exception? exception)
        {
            if (exception == null) return string.Empty;

            var innerDetails = new StringBuilder();
            var innerException = exception.InnerException;
            while (innerException != null)
            {
                innerDetails.AppendLine($"Inner Exception: {innerException.GetType().Name} - {innerException.Message}");
                innerDetails.AppendLine($"Stack Trace: {ExtractShortStackTrace(innerException.StackTrace)}");
                innerException = innerException.InnerException;
            }

            return innerDetails.ToString();
        }

        /// <summary>
        /// Phân tích chi tiết stack trace, lấy thông tin quan trọng như tên lớp, tên phương thức và số dòng.
        /// </summary>
        public static string AnalyzeStackTrace(string? stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace)) return "No Stack Trace available.";

            var stackLines = stackTrace.Split('\n');
            var details = new StringBuilder();

            foreach (var line in stackLines)
            {
                // Kiểm tra nếu dòng chứa thông tin quan trọng
                if (line.Contains("at"))
                {
                    var methodInfo = ExtractMethodDetails(line);
                    details.AppendLine(methodInfo);
                }
            }

            return details.ToString();
        }

        public static string AnalyzeStackTrace(string? stackTrace, int startFromLine = 0)
        {
            if (string.IsNullOrEmpty(stackTrace)) return "No Stack Trace available.";

            var stackLines = stackTrace.Split('\n');
            var details = new StringBuilder();

            // Lặp qua các dòng trong stack trace bắt đầu từ dòng startFromLine
            for (int i = startFromLine; i < stackLines.Length; i++)
            {
                var line = stackLines[i].Trim(); // Loại bỏ khoảng trắng dư thừa

                // Kiểm tra nếu dòng chứa thông tin phương thức
                if (line.Contains("at"))
                {
                    var methodInfo = ExtractMethodDetails(line);
                    details.AppendLine(methodInfo);
                }
            }

            return details.ToString();
        }

        /// <summary>
        /// Trích xuất thông tin tên phương thức và dòng trong stack trace.
        /// </summary>
        private static string ExtractMethodDetails(string line)
        {
            // Dòng chứa thông tin phương thức, ví dụ: "at Namespace.Class.MethodName()"
            var methodStart = line.IndexOf("at ");
            if (methodStart < 0) return "Unknown method";

            var methodLine = line.Substring(methodStart + 3); // Bỏ qua "at "
            var methodParts = methodLine.Split('(');

            if (methodParts.Length > 1)
            {
                return $"{methodParts[0]} - Line: {methodParts[1]}";
            }

            return methodLine;
        }

        private static string ExtractLastRelevantSegment(string input, char separator1, char separator2)
        {
            var lastSeparator1Position = input.LastIndexOf(separator1);
            var lastSeparator2Position = input.LastIndexOf(separator2);

            if (lastSeparator1Position >= 0 && lastSeparator2Position > lastSeparator1Position)
                return input[(lastSeparator1Position + 1)..];

            return input; // Trả về toàn bộ nếu không tìm thấy thông tin cần thiết
        }

        private static string ExtractLastSegment(string input, char separator)
        {
            var lastSeparatorPosition = input.LastIndexOf(separator);
            return lastSeparatorPosition >= 0 ? input[(lastSeparatorPosition + 1)..] : input;
        }
    }
}