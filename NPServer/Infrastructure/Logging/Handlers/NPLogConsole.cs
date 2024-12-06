using NPServer.Infrastructure.Logging.Formatter;
using NPServer.Infrastructure.Logging.Interfaces;
using System;

namespace NPServer.Infrastructure.Logging.Handlers;

public class NPLogConsole(INPLogFormatter loggerFormatter) : INPLogHandler
{
    private readonly INPLogFormatter _loggerFormatter = loggerFormatter;

    public NPLogConsole() : this(new NPLogFormatter())
    { }

    public void Publish(NPLogMessage logMessage) =>
        Console.WriteLine(_loggerFormatter.ApplyFormat(logMessage));
}