using NPServer.Shared.Default;
using System;
using System.IO;

namespace NPServer.Infrastructure.Logging;

public static class NPLogCongfig
{
    public static readonly string LogDirectory = Path.Combine(PathConfig.LogFolder, DateTime.Now.ToString("yyMMdd"));
}