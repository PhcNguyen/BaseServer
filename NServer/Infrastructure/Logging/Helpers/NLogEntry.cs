using System;
using System.Text;

using Base.Infrastructure.Logging.Enums;
using Base.Infrastructure.Logging.Filter;

namespace Base.Infrastructure.Logging.Helpers
{
    public class NLogEntry(LogLevel level, string? message = null, Exception? exception = null, string? additionalInfo = null)
    {
        public readonly DateTime Timestamp = DateTime.UtcNow;

        public LogLevel Level { get; set; } = level;
        public Exception? Exception { get; set; } = exception;

        public string? Message { get; set; } = message;
        public string? Additional { get; set; } = additionalInfo;

        public string ToStrings()
        {
            var log = new StringBuilder(256);

            log.AppendFormat("{0:yy/MM/dd HH:mm:ss} - {1}", Timestamp, Level);

            if (!string.IsNullOrEmpty(Message))
            {
                log.Append(" - ").Append(Message);
            }

            if (!string.IsNullOrEmpty(Additional))
            {
                log.Append(" - ADDITIONAL: ").Append(Additional);
            }

            if (Exception != null)
            {
                log.Append(" - EXCEPTION: ").Append(Exception.Message)
                   .Append(" | SOURCE: ").Append(FilterException.ExtractSourceFileName(Exception.Source))
                   .Append(" | STACKTRACE: ").Append(FilterException.ExtractShortStackTrace(Exception.StackTrace))
                   .Append(" | TYPE: ").Append(FilterException.ExtractSourceFileName(Exception.GetType().FullName));

                //string stackTraceDetails = FilterException.AnalyzeStackTrace(Exception.StackTrace);

                //log.Append(" | STACKTRACE: ").Append(stackTraceDetails)
                //   .Append(" | TYPE: ").Append(FilterException.ExtractSourceFileName(Exception.GetType().FullName));
            }

            return log.ToString();
        }
    }
}