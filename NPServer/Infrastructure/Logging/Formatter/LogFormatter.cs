﻿using NPServer.Infrastructure.Logging.Interfaces;

namespace NPServer.Infrastructure.Logging.Formatter
{
    public class LogFormatter : ILogFormatter
    {
        private readonly string _message = "{0:HH:mm:ss.fff} - {1} - {2}{3}";

        public string ApplyFormat(LogMessage logMessage)
        {
            // Kiểm tra và xây dựng chuỗi log chỉ khi CallingClass và CallingMethod có giá trị hợp lệ
            string callingInfo = string.Empty;

            if (!string.IsNullOrEmpty(logMessage.CallingClass) && !string.IsNullOrEmpty(logMessage.CallingMethod))
            {
                callingInfo = $"[{logMessage.CallingClass} -> {logMessage.CallingMethod}()]: ";
            }

            // Format message với hoặc không có CallingClass/Method
            return string.Format(_message,
                logMessage.DateTime, logMessage.Level, callingInfo, logMessage.Text);
        }

        public static string FormatExceptionMessage(System.Exception exception) =>
            $"Log exception -> Message: {exception.Message}\nStackTrace: {exception.StackTrace}";
    }
}