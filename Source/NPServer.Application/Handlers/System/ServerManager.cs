using NPServer.Application.Threading;
using NPServer.Common.Models;
using NPServer.Core.Commands;
using NPServer.Shared.Services;

namespace NPServer.Application.Handlers.System;

internal static class ServerManager
{
    private static readonly ServerApp _serverApp = Singleton.GetInstance<ServerApp>();

    [Command(Command.Restart, Authoritys.Administrator)]
    public static bool Restart()
    {
        try
        {
            _serverApp.Reset();
            return true;
        }
        catch
        {
            return false;
        }
    }

    [Command(Command.Shutdown, Authoritys.Administrator)]
    public static bool Shutdown()
    {
        try
        {
            _serverApp.Shutdown();
            return true;
        }
        catch
        {
            return false;
        }
    }

    [Command(Command.Status, Authoritys.Administrator)]
    public static bool Status()
    {
        try
        {
            return true;
        }
        catch
        {
            return false;
        }
    }
}