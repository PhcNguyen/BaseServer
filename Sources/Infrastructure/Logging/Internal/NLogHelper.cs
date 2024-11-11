namespace NETServer.Logging.Internal;

internal class NLogHelper
{
    private static readonly string LogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
    public static string LogFolderDate = Path.Combine(LogFolder, DateTime.Now.ToString("yyMMdd"));
    public static string DefaultFilePath = Path.Combine(NLogHelper.LogFolderDate, "default.log");
    public static int LineNumber = 0;

    internal static readonly Dictionary<NLogLevel, string> LevelFileMapping = new Dictionary<NLogLevel, string>
    {
        { NLogLevel.Critical, Path.Combine(NLogHelper.LogFolderDate, "critical.log") },
        { NLogLevel.Error, Path.Combine(NLogHelper.LogFolderDate, "error.log") },
        { NLogLevel.Warning, Path.Combine(NLogHelper.LogFolderDate, "warning.log") },
        { NLogLevel.Info, Path.Combine(NLogHelper.LogFolderDate, "information.log") },
    };

    static NLogHelper() => EnsureLogDirectoryExists();

    private static void EnsureLogDirectoryExists()
    {
        if (!Directory.Exists(LogFolder))
        {
            Directory.CreateDirectory(LogFolder);
        }

        if (!Directory.Exists(LogFolderDate))
        {
            Directory.CreateDirectory(LogFolderDate);
        }
    }

    internal static async Task LogMessage(string? message, NLogLevel level, Exception? exception = null)
    {
        string formattedMessage = NLogFormatter.FormatMessage(message, level, exception);
        await NLogFileHandler.WriteLogAsync(formattedMessage, level);
    }

    
}
