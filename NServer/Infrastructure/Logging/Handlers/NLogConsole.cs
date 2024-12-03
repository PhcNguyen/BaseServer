using NServer.Infrastructure.Logging.Formatter;
using NServer.Infrastructure.Logging.Interfaces;
using System;

namespace NServer.Infrastructure.Logging.Handlers;

public class NLogConsole(INLogFormatter loggerFormatter) : INLogHandler
{
    private readonly INLogFormatter _loggerFormatter = loggerFormatter;

    public NLogConsole() : this(new NLogFormatter())
    { }

    public void Publish(LogMessage logMessage) =>
        Console.WriteLine(_loggerFormatter.ApplyFormat(logMessage));
}