using NETServer.Infrastructure.Configuration;

namespace NETServer.Infrastructure.Logging.Helpers
{
    internal class FileManager
    {
        public static string CurrentLogDirectory = Path.Combine(LoggingConfigs.LogFolder, DateTime.Now.ToString("yyMMdd"));

        internal static readonly Dictionary<LogLevel, string> LogLevelFileMappings = new Dictionary<LogLevel, string>
        {
            { LogLevel.Info, Path.Combine(CurrentLogDirectory, "info.log") },
            { LogLevel.Error, Path.Combine(CurrentLogDirectory, "error.log") },
            { LogLevel.Warning, Path.Combine(CurrentLogDirectory, "warning.log") },
            { LogLevel.Critical, Path.Combine(CurrentLogDirectory, "critical.log") },
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
            message = BuildMessageLog.BuildLogMessage(message, level, exception);
            FileLogging.QueueLog(message, level);

            if (LoggingConfigs.ConsoleLogging)
            {
                Console.WriteLine(message);
            }
        }
        
    }
}