using NPServer.Infrastructure.Logging.Formatter;
using NPServer.Infrastructure.Logging.Interfaces;
using System;

namespace NPServer.Infrastructure.Logging.Targets
{
    public class ConsoleTarget(INPLogFormatter loggerFormatter) : INPLogTarget
    {
        private readonly INPLogFormatter _loggerFormatter = loggerFormatter;

        public ConsoleTarget() : this(new NPLogFormatter())
        { }

        public void Publish(NPLogMessage logMessage)
        {
            SetForegroundColor(logMessage.Level);
            Console.WriteLine(_loggerFormatter.ApplyFormat(logMessage));
            Console.ResetColor();
        }

        private static void SetForegroundColor(NPLog.Level level)
        {
            switch (level)
            {
                case NPLog.Level.NONE: Console.ForegroundColor = ConsoleColor.Cyan; break;
                case NPLog.Level.INFO: Console.ForegroundColor = ConsoleColor.White; break;
                case NPLog.Level.WARNING: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case NPLog.Level.ERROR: Console.ForegroundColor = ConsoleColor.Magenta; break;
                case NPLog.Level.CRITICAL: Console.ForegroundColor = ConsoleColor.Red; break;
            }
        }
    }
}