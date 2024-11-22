using NServer.Infrastructure.Configuration;
using NServer.Infrastructure.Logging.Enums;

namespace NServer.Infrastructure.Logging.Helpers
{
    internal class FileManager
    {
        public static string CurrentLogDirectory = Path.Combine(LoggingConfigs.LogFolder, DateTime.Now.ToString("yyMMdd"));

        internal static readonly Dictionary<LogLevel, string> LogLevelFileMappings = new()
        {
            { LogLevel.INFO, Path.Combine(CurrentLogDirectory, "info.log") },
            { LogLevel.ERROR, Path.Combine(CurrentLogDirectory, "error.log") },
            { LogLevel.WARNING, Path.Combine(CurrentLogDirectory, "warning.log") },
            { LogLevel.CRITICAL, Path.Combine(CurrentLogDirectory, "critical.log") },
        };

        public static string DefaultLogFilePath => Path.Combine(CurrentLogDirectory, "default.log");

        static FileManager() => EnsureLogDirectoriesExist();

        public static void RefreshLogDirectory()
        {
            CurrentLogDirectory = Path.Combine(LoggingConfigs.LogFolder, DateTime.Now.ToString("yyMMdd"));

            if (!Directory.Exists(CurrentLogDirectory))
            {
                Directory.CreateDirectory(CurrentLogDirectory);
            }
        }

        private static void EnsureLogDirectoriesExist()
        {
            if (!Directory.Exists(LoggingConfigs.LogFolder))
            {
                Directory.CreateDirectory(LoggingConfigs.LogFolder);
            }

            if (!Directory.Exists(CurrentLogDirectory))
            {
                Directory.CreateDirectory(CurrentLogDirectory);
            }
        }

        internal static void WriteLogToFile(string? message, LogLevel level, Exception? exception = null)
        {
            NLogEntry logging = new(level, message, exception);
            FileLogging.QueueLog(logging.ToStrings(), level);

            if (LoggingConfigs.ConsoleLogging)
            {
                Console.WriteLine(message);
            }
        }

    }
}