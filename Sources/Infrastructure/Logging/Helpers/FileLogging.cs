using System.Text;

namespace NETServer.Infrastructure.Logging.Helpers;

internal static class FileLogging
{
    private static string GetFilePath(LogLevel level) =>
        FileManager.LogLevelFileMappings.GetValueOrDefault(level, FileManager.DefaultLogFilePath);

    public static async Task WriteLogAsync(string message, LogLevel level)
    {
        try
        {
            string filePath = GetFilePath(level);
            using var writer = new StreamWriter(new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8, bufferSize: 1024, leaveOpen: false);
            await writer.WriteLineAsync(message);
        }
        catch (Exception)
        {
            // throw new InvalidOperationException("An error occurred while writing the log.", ex);
        }
    }
}
