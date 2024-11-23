using NServer.Infrastructure.Logging.Enums;
using NServer.Infrastructure.Configuration;

namespace NServer.Infrastructure.Logging.Helpers
{
    internal class Config
    {
        public readonly static string CurrentLogDirectory = Path.Combine(LoggingConfigs.LogFolder, DateTime.Now.ToString("yyMMdd"));

        public readonly static string DefaultLogFilePath = Path.Combine(CurrentLogDirectory, "default.log");

        public readonly static Dictionary<LogLevel, string> LogLevelFileMappings = new()
        {
            { LogLevel.INFO, Path.Combine(CurrentLogDirectory, "info.log") },
            { LogLevel.ERROR, Path.Combine(CurrentLogDirectory, "error.log") },
            { LogLevel.WARNING, Path.Combine(CurrentLogDirectory, "warning.log") },
            { LogLevel.CRITICAL, Path.Combine(CurrentLogDirectory, "critical.log") },
        };
    }
}
