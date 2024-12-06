using NPServer.Infrastructure.Logging.Formatter;

namespace NPServer.Infrastructure.Logging.Interfaces;

public interface INLogHandler
{
    void Publish(LogMessage logMessage);
}