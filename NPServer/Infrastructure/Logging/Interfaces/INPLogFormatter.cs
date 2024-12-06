using NPServer.Infrastructure.Logging.Formatter;

namespace NPServer.Infrastructure.Logging.Interfaces;

public interface INPLogFormatter
{
    string ApplyFormat(NPLogMessage logMessage);
}