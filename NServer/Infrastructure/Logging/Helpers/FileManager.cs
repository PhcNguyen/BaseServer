using NServer.Infrastructure.Configuration;
using NServer.Infrastructure.Logging.Enums;

namespace NServer.Infrastructure.Logging.Helpers
{
    internal class FileManager
    {
        private readonly FileLogging _fileLogging;

        public FileManager()
        {
            EnsureLogDirectoriesExist();
            _fileLogging = new FileLogging();
        }

        public static void RefreshLogDirectory()
        {

            if (!Directory.Exists(Config.CurrentLogDirectory))
            {
                Directory.CreateDirectory(Config.CurrentLogDirectory);
            }
        }

        private static void EnsureLogDirectoriesExist()
        {
            if (!Directory.Exists(LoggingConfigs.LogFolder))
            {
                Directory.CreateDirectory(LoggingConfigs.LogFolder);
            }

            if (!Directory.Exists(Config.CurrentLogDirectory))
            {
                Directory.CreateDirectory(Config.CurrentLogDirectory);
            }
        }

        public void Start() => _fileLogging.Start();

        public void Dispose() => _fileLogging.Dispose();

        public void WriteLogToFile(string? message, LogLevel level, Exception? exception = null)
        {
            NLogEntry logging = new(level, message, exception);
            _fileLogging.QueueLog(logging.ToStrings(), level);

            if (LoggingConfigs.ConsoleLogging)
            {
                Console.WriteLine(message);
            }
        }
    }
}