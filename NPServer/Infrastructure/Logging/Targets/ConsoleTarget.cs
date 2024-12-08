using NPServer.Infrastructure.Logging.Formatter;
using NPServer.Infrastructure.Logging.Interfaces;

namespace NPServer.Infrastructure.Logging.Targets
{
    public class ConsoleTarget(ILogFormatter loggerFormatter) : INPLogTarget
    {
        private readonly ILogFormatter _loggerFormatter = loggerFormatter;

        public ConsoleTarget() : this(new LogFormatter())
        { }

        public void Publish(LogMessage logMessage)
        {
            SetForegroundColor(logMessage.Level);
            System.Console.WriteLine(_loggerFormatter.ApplyFormat(logMessage));
            System.Console.ResetColor();
        }

        private static void SetForegroundColor(NPLog.Level level)
        {
            System.Console.ForegroundColor = level switch
            {
                NPLog.Level.NONE => System.ConsoleColor.Cyan,
                NPLog.Level.INFO => System.ConsoleColor.White,
                NPLog.Level.WARNING => System.ConsoleColor.Yellow,
                NPLog.Level.ERROR => System.ConsoleColor.Magenta,
                NPLog.Level.CRITICAL => System.ConsoleColor.Red,
                _ => System.ConsoleColor.White,
            };
        }
    }
}