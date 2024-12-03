using NServer.Infrastructure.Logging.Formatter;

namespace NServer.Infrastructure.Logging.Interfaces;

public interface INLogFormatter
{
    string ApplyFormat(LogMessage logMessage);
}