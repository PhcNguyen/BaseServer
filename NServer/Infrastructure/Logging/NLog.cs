using System;

using NServer.Infrastructure.Services;
using NServer.Infrastructure.Logging.Enums;
using NServer.Infrastructure.Logging.Helpers;

namespace NServer.Infrastructure.Logging
{
    /// <summary>
    /// Provides logging functionalities with different levels.
    /// </summary>
    public class NLog
    {
        private static readonly FileManager _fileManager = Singleton.GetInstance<FileManager>();

        /// <summary>
        /// Logs a message with a specified level and optional exception.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <param name="level">The log level.</param>
        /// <param name="exception">An optional exception.</param>
        private static void Log(LogLevel level, string? message, Exception? exception = null)
        {
            _fileManager.WriteLogToFile(message, level, exception);
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        public static void Info(string message) => Log(LogLevel.INFO, message);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        public static void Error(string message) => Log(LogLevel.ERROR, message);

        /// <summary>
        /// Logs an error message with an exception.
        /// </summary>
        public static void Error(Exception exception) =>
            Log(LogLevel.ERROR, null, exception);

        public static void Error(string message, Exception exception) => 
            Log(LogLevel.ERROR, message, exception);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        public static void Warning(string message) => Log(LogLevel.WARNING, message);

        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        public static void Warning(Exception exception) =>
            Log(LogLevel.WARNING, null, exception);
    }
}