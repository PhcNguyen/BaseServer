using NPServer.Infrastructure.Logging.Interfaces;
using System;
using System.Reflection.Emit;

namespace NPServer.Infrastructure.Logging.Formatter;

public class NPLogFormatter : INPLogFormatter
{
    private readonly string _message = "{0:HH:mm:ss.fff} - {1} - [{2} -> {3}()]: {4}";

    public string ApplyFormat(NPLogMessage logMessage)
    {
        return string.Format(_message,
            logMessage.DateTime, logMessage.Level, logMessage.CallingClass,
            logMessage.CallingMethod, logMessage.Text);
    }

    public static string FormatExceptionMessage(Exception exception) =>
        $"Log exception -> Message: {exception.Message}\nStackTrace: {exception.StackTrace}";
}