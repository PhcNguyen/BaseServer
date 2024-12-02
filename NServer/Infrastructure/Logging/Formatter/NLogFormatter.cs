﻿using NServer.Infrastructure.Logging.Interfaces;
using System;

namespace NServer.Infrastructure.Logging.Formatter;

public class NLogFormatter : INLogFormatter
{
    private readonly string _message = "{0:dd.MM.yyyy HH:mm:ss} - {1} - [{2} -> {3}()]: {4}";

    public string ApplyFormat(LogMessage logMessage)
    {
        return string.Format(_message,
            logMessage.DateTime, logMessage.Level, logMessage.CallingClass,
            logMessage.CallingMethod, logMessage.Text);
    }

    public static string FormatExceptionMessage(Exception exception) =>
        $"Log exception -> Message: {exception.Message}\nStackTrace: {exception.StackTrace}";
}