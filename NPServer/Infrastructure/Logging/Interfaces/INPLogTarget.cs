using NPServer.Infrastructure.Logging.Formatter;

namespace NPServer.Infrastructure.Logging.Interfaces
{
    public interface INPLogTarget
    {
        void Publish(NPLogMessage logMessage);
    }
}