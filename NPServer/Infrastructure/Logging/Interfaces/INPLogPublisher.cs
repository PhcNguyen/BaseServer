using NPServer.Infrastructure.Logging.Formatter;
using System;

namespace NPServer.Infrastructure.Logging.Interfaces;

public interface INPLogPublisher
{
    INPLogPublisher AddHandler(INPLogHandler loggerHandler);

    INPLogPublisher AddHandler(INPLogHandler loggerHandler, Predicate<NPLogMessage> filter);

    bool RemoveHandler(INPLogHandler loggerHandler);
}