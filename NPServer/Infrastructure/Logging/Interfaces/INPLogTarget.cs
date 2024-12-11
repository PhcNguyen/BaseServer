using NPServer.Infrastructure.Logging.Formatter;

namespace NPServer.Infrastructure.Logging.Interfaces
{
    public interface INPLogTarget
    {
        void Publish(LogMessage logMessage);
        void Dispose();
    }
}