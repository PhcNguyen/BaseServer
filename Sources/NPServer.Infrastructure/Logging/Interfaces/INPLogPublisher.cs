using NPServer.Infrastructure.Logging.Formatter;
using System;

namespace NPServer.Infrastructure.Logging.Interfaces;

public interface INPLogPublisher
{
    INPLogPublisher AddHandler(INPLogTarget loggerHandler);

    INPLogPublisher AddHandler(INPLogTarget loggerHandler, Predicate<LogMessage> filter);

    bool RemoveHandler(INPLogTarget loggerHandler);
}