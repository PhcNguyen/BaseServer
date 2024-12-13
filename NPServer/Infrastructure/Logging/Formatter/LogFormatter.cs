using NPServer.Infrastructure.Logging.Interfaces;
using System;
using System.Text;

namespace NPServer.Infrastructure.Logging.Formatter
{
    public class LogFormatter : ILogFormatter
    {
        public LogFormatter()
        {
        }

        public string ApplyFormat(LogMessage logMessage)
        {
            ArgumentNullException.ThrowIfNull(logMessage);

            StringBuilder logBuilder = new();

            logBuilder.AppendFormat("{0:HH:mm:ss.fff}", logMessage.DateTime);
            logBuilder.Append(" - ");
            logBuilder.Append(logMessage.Level);
            logBuilder.Append(" - ");

            if (!string.IsNullOrEmpty(logMessage.CallingClass) && !string.IsNullOrEmpty(logMessage.CallingMethod))
            {
                logBuilder.AppendFormat("[{0} -> {1}()]: ", logMessage.CallingClass, logMessage.CallingMethod);
            }

            logBuilder.Append(logMessage.Text);

            return logBuilder.ToString();
        }

        public static string FormatExceptionMessage(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            StringBuilder exceptionBuilder = new();
            exceptionBuilder.AppendLine("Log exception -> ");
            exceptionBuilder.AppendLine($"Message: {exception.Message}");
            exceptionBuilder.AppendLine($"StackTrace: {exception.StackTrace}");

            if (exception.InnerException != null)
            {
                exceptionBuilder.AppendLine($"InnerException: {exception.InnerException.Message}");
                exceptionBuilder.AppendLine($"InnerException StackTrace: {exception.InnerException.StackTrace}");
            }

            return exceptionBuilder.ToString();
        }
    }
}