using NPServer.Infrastructure.Logging.Formatter;

namespace NPServer.Infrastructure.Logging.Interfaces;

public interface ILogFormatter
{
    string ApplyFormat(LogMessage logMessage);
}