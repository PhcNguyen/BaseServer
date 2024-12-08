using NPServer.Infrastructure.Logging.Formatter;

namespace NPServer.Infrastructure.Logging.Interfaces
{
    public interface INPLogHandler
    {
        void Publish(NPLogMessage logMessage);
    }
}