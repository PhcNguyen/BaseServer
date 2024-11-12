using NETServer.Infrastructure.Logging.Helpers;

namespace NETServer.Infrastructure.Logging;

/// <summary>
/// Provides logging functionalities with different levels.
/// </summary>
public class NLog
{
    /// <summary>
    /// Logs a message with a specified level and optional exception.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="level">The log level.</param>
    /// <param name="exception">An optional exception.</param>
    private static void Log(string? message, LogLevel level, Exception? exception = null) =>
        Task.Run(() => FileManager.WriteLogToFileAsync(message, level, exception));
    
    /// <summary>
    /// Logs an informational message.
    /// </summary>
    public static void Info(string message) => Log(message, LogLevel.Info);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    public static void Error(string message) => Log(message, LogLevel.Error);

    /// <summary>
    /// Logs an error message with an exception.
    /// </summary>
    public static void Error(Exception exception) =>
        Log(null, LogLevel.Error, exception);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public static void Warning(string message) => Log(message, LogLevel.Warning);

    /// <summary>
    /// Logs a warning message with an exception.
    /// </summary>
    public static void Warning(Exception exception) =>
        Log(null, LogLevel.Warning, exception);
}
