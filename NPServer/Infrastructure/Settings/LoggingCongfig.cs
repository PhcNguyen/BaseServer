using System;
using System.IO;

namespace NPServer.Infrastructure.Settings
{
    internal static class LoggingCongfig
    {
        public static readonly string LogDirectory = Path.Combine(PathConfig.LogFolder, DateTime.Now.ToString("yyMMdd"));
    }
}