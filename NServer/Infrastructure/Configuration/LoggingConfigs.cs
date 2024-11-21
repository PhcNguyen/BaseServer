namespace NServer.Infrastructure.Configuration
{
    internal class LoggingConfigs
    {
        public readonly static string LogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        public readonly static bool ConsoleLogging = true;
        public readonly static int BatchSize = 10;
        public static TimeSpan InitialFlushDelay { get; set; } = TimeSpan.FromSeconds(5);  // Thời gian chờ trước lần gọi đầu tiên
        public static TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(5);      // Khoảng thời gian lặp 
    }
}
