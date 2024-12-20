using System;
using System.IO;

namespace NPServer.Infrastructure.Settings;

public static class LoggingCongfig
{
    public static string LogDirectory = Path.Combine(PathConfig.LogFolder, DateTime.Now.ToString("yyMMdd"));
}