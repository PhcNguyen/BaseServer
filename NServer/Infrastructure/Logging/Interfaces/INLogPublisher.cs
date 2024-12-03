using NServer.Infrastructure.Logging.Formatter;
using System;

namespace NServer.Infrastructure.Logging.Interfaces;

public interface INLogPublisher
{
    INLogPublisher AddHandler(INLogHandler loggerHandler);

    INLogPublisher AddHandler(INLogHandler loggerHandler, Predicate<LogMessage> filter);

    bool RemoveHandler(INLogHandler loggerHandler);
}