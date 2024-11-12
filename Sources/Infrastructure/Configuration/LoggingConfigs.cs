namespace NETServer.Infrastructure.Configuration
{
    internal class LoggingConfigs
    {
        public readonly static string LogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        public readonly static bool ConsoleLogging = true;

    }
}
