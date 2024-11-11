using NETServer.Logging.Internal;

namespace NETServer.Logging;

/// <summary>
/// Provides logging functionalities with different levels.
/// </summary>
public class NLog
{
    /// <summary>
    /// Gets or sets a value indicating the console log level.
    /// </summary>
    public static NLogLevel ConsoleLogLevel { get; set; } = NLogLevel.Info;

    /// <summary>
    /// Gets or sets a value indicating the file write log level.
    /// </summary>
    public static NLogLevel WriteLogLevel { get; set; } = NLogLevel.Info;

    /// <summary>
    /// Logs a message with a specified level and optional exception.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="level">The log level.</param>
    /// <param name="exception">An optional exception.</param>
    public static void Log(string? message, NLogLevel level, Exception? exception = null) =>
        Task.Run(() => Internal.NLogHelper.LogMessage(message, level, exception));

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The log message.</param>
    public static void Info(string message) => Log(message, NLogLevel.Info);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The log message.</param>
    public static void Error(string message) => Log(message, NLogLevel.Error);

    /// <summary>
    /// Logs an error message with an exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    public static void Error(Exception exception) => Log(exception.Message, NLogLevel.Error, exception);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The log message.</param>
    public static void Warning(string message) => Log(message, NLogLevel.Warning);

    /// <summary>
    /// Logs a warning message with an exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    public static void Warning(Exception exception) => Log(exception.Message, NLogLevel.Warning);

    /// <summary>
    /// Logs an error message with an exception and custom message.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="message">The custom log message.</param>
    public static void Error(Exception exception, string message) => Log(message, NLogLevel.Error);

    /// <summary>
    /// Logs a warning message with an exception and custom message.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="message">The custom log message.</param>
    public static void Warning(Exception exception, string message) => Log(message, NLogLevel.Warning, exception);

    public static void Pause() => NLogFileHandler.PauseNLog();
    public static void Resume() => NLogFileHandler.ResumeNLog();
}
