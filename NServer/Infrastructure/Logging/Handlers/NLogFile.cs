using NServer.Infrastructure.Configuration;
using NServer.Infrastructure.Logging.Enums;
using NServer.Infrastructure.Logging.Formatter;
using NServer.Infrastructure.Logging.Interfaces;
using System;
using System.IO;

namespace NServer.Infrastructure.Logging.Handlers
{
    public class NLogFile(INLogFormatter loggerFormatter, string directory) : INLogHandler
    {
        private readonly string _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        private readonly INLogFormatter _loggerFormatter = loggerFormatter ?? throw new ArgumentNullException(nameof(loggerFormatter));
        private static readonly object _lock = new(); // Đối tượng khóa dùng để đồng bộ hóa việc ghi log

        public NLogFile() : this(new NLogFormatter(), LoggingCongfig.LogDirectory)
        {
        }

        public NLogFile(string directory) : this(new NLogFormatter(), directory)
        {
        }

        public void Publish(LogMessage logMessage)
        {
            // Kiểm tra và tạo thư mục nếu chưa có
            if (!string.IsNullOrEmpty(_directory))
            {
                DirectoryInfo directoryInfo = new(Path.Combine(_directory));
                if (!directoryInfo.Exists) directoryInfo.Create();
            }

            // Tạo tên file dựa trên mức độ log
            string filePath = Path.Combine(_directory, CreateFileName(logMessage.Level));

            // Đảm bảo việc ghi log là thread-safe bằng cách sử dụng khóa
            lock (_lock)
            {
                using StreamWriter writer = new(File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read));
                writer.WriteLine(_loggerFormatter.ApplyFormat(logMessage));
            }
        }

        private static string CreateFileName(NLogLevel level) =>
           string.Format("{0}.log", level.ToString());
    }
}