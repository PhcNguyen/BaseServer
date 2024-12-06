using NPServer.Infrastructure.Logging.Formatter;
using System;

namespace NPServer.Infrastructure.Logging.Interfaces;

public interface INLogPublisher
{
    INLogPublisher AddHandler(INLogHandler loggerHandler);

    INLogPublisher AddHandler(INLogHandler loggerHandler, Predicate<LogMessage> filter);

    bool RemoveHandler(INLogHandler loggerHandler);
}