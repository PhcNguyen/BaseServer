using System;
using System.IO;
using Base.Infrastructure.Logging.Enums;
using Base.Infrastructure.Logging.Helpers;

namespace Base.Infrastructure.Logging
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
            if (!Directory.Exists(Config.LogFolder))
            {
                Directory.CreateDirectory(Config.LogFolder);
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

            string? mess = logging.ToStrings();

            _fileLogging.QueueLog(mess, level);

            if (Config.ConsoleLogging)
            {
                Console.WriteLine(mess);
            }
        }
    }
}