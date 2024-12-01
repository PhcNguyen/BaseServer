using System;
using System.IO;

namespace NServer.Infrastructure.Configuration
{
    internal static class LoggingCongfig
    {
        private static readonly string _baseLogs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        public static readonly string LogDirectory = Path.Combine(_baseLogs, DateTime.Now.ToString("yyMMdd"));

        public static readonly long MaxFileSize = 5 * 1024 * 1024; // Kích thước tối đa 5MB
    }
}