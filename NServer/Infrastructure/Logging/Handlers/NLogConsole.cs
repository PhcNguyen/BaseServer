using NPServer.Infrastructure.Logging.Formatter;
using NPServer.Infrastructure.Logging.Interfaces;
using System;

namespace NPServer.Infrastructure.Logging.Handlers;

public class NLogConsole(INLogFormatter loggerFormatter) : INLogHandler
{
    private readonly INLogFormatter _loggerFormatter = loggerFormatter;

    public NLogConsole() : this(new NLogFormatter())
    { }

    public void Publish(LogMessage logMessage) =>
        Console.WriteLine(_loggerFormatter.ApplyFormat(logMessage));
}