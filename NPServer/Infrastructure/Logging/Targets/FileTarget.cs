using NPServer.Infrastructure.Logging.Formatter;
using NPServer.Infrastructure.Logging.Interfaces;
using NPServer.Infrastructure.Settings;
using System;
using System.IO;
using System.Threading;

namespace NPServer.Infrastructure.Logging.Targets
{
    public class FileTarget(INPLogFormatter loggerFormatter, string directory) : INPLogTarget
    {
        private readonly string _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        private readonly INPLogFormatter _loggerFormatter = loggerFormatter ?? throw new ArgumentNullException(nameof(loggerFormatter));
        private static readonly Lock _lock = new(); // Đối tượng khóa dùng để đồng bộ hóa việc ghi log

        public FileTarget() : this(new NPLogFormatter(), LoggingCongfig.LogDirectory)
        {
        }

        public FileTarget(string directory) : this(new NPLogFormatter(), directory)
        {
        }

        public void Publish(NPLogMessage logMessage)
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

        private static string CreateFileName(NPLog.Level level) =>
            string.Format("{0}.log", level.ToString());
    }
}