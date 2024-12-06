using NPServer.Infrastructure.Logging.Formatter;

namespace NPServer.Infrastructure.Logging.Interfaces;

public interface INLogFormatter
{
    string ApplyFormat(LogMessage logMessage);
}