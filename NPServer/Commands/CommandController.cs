using NPServer.Commands.Abstract;
using NPServer.Commands.Interfaces;
using NPServer.Infrastructure.Logging;

namespace NPServer.Commands;

internal sealed class CommandController : AbstractCommandDispatcher
{
    private static readonly string[] TargetNamespaces =
    [
        "NServer.Application.Handlers.Implementations",
    ];

    public CommandController() : base(TargetNamespaces)
    {
    }

    public (object, object?) HandleCommand(ICommandInput input)
    {
        if (!CommandDelegateCache.TryGetValue(input.Command, out var commandInfo))
        {
            return ($"Unknown command: {input.Command}", null);
        }

        var (requiredRole, func) = commandInfo;

        if (input.UserRole < requiredRole)
        {
            return ($"Permission denied for command: {input.Command}", null);
        }

        try
        {
            if (func(input) is not object result)
                throw new System.InvalidOperationException("Invalid result type from command handler.");

            return (result, null);
        }
        catch (System.Exception ex)
        {
            NPLog.Instance.Error<CommandController>(
                $"Error executing command: {input.Command}. Exception: {ex.Message}");
            return ($"Error executing command: {input.Command}", null);
        }
    }
}