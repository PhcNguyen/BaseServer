using NServer.Infrastructure.Logging.Formatter;

namespace NServer.Infrastructure.Logging.Interfaces
{
    public interface INLogHandler
    {
        void Publish(LogMessage logMessage);
    }
}