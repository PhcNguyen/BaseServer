using System;
using System.IO;

namespace NPServer.Infrastructure.Logging;

internal static class LoggingCongfig
{
    private static readonly string _baseLogs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
    public static readonly string LogDirectory = Path.Combine(_baseLogs, DateTime.Now.ToString("yyMMdd"));
}