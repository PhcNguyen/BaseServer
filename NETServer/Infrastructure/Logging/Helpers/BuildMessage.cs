using System.Text;

namespace NETServer.Infrastructure.Logging.Helpers
{
    internal static class BuildMessageLog
    {
        /// <summary>
        /// Xây dựng thông điệp log với mức độ log, thông điệp và thông tin ngoại lệ nếu có.
        /// </summary>
        public static string BuildLogMessage(string? message, LogLevel level, Exception? exception)
        {
            var logMessageBuilder = new StringBuilder();

            logMessageBuilder.Append($"{ DateTime.UtcNow:HH:mm:ss} - ");
            logMessageBuilder.Append(level.ToString().ToUpperInvariant());

            // Nếu có thông điệp, thêm vào log
            if (!string.IsNullOrEmpty(message))
            {
                logMessageBuilder.Append($" - {message}");
            }

            // Nếu có ngoại lệ và mức độ log là Error trở lên, thêm chi tiết về ngoại lệ
            if (exception != null)
            {
                logMessageBuilder.Append(BuildExceptionLog.FormatExceptionDetails(exception));
            }

            return logMessageBuilder.ToString();
        }
    }
}
