using System;
using System.IO;
using System.Collections.Generic;

using NServer.Infrastructure.Logging.Enums;

namespace NServer.Infrastructure.Logging
{
    internal class Config
    {
        public readonly static string LogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        public readonly static bool ConsoleLogging = true;
        public readonly static int BatchSize = 10;
        public static TimeSpan InitialFlushDelay { get; set; } = TimeSpan.FromSeconds(5);  // Thời gian chờ trước lần gọi đầu tiên
        public static TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(10);      // Khoảng thời gian lặp 

        public readonly static string CurrentLogDirectory = Path.Combine(LogFolder, DateTime.Now.ToString("yyMMdd"));

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
