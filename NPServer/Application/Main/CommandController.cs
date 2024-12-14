using NPServer.Commands;
using NPServer.Commands.Abstract;
using NPServer.Commands.Interfaces;
using NPServer.Infrastructure.Logging;

namespace NPServer.Application.Main
{
    internal class CommandController : AbstractCommandDispatcher
    {
        private static readonly string[] TargetNamespaces =
        [
            "NServer.Application.Handlers.Implementations",
        ];

        public CommandController() : base(TargetNamespaces)
        {
        }

        public (CommandExecutionResult, object?) HandleCommand(ICommandInput input)
        {
            if (!CommandDelegateCache.TryGetValue(input.Command, out var commandInfo))
            {
                return (CommandExecutionResult.Error($"Unknown command: {input.Command}"), null);
            }

            var (requiredRole, func) = commandInfo;

            if (input.UserRole < requiredRole)
            {
                return (CommandExecutionResult.Error($"Permission denied for command: {input.Command}"), null);
            }

            try
            {
                if (func(input) is not object result)
                    throw new System.InvalidOperationException("Invalid result type from command handler.");

                return (CommandExecutionResult.Success(result), null);
            }
            catch (System.Exception ex)
            {
                NPLog.Instance.Error<CommandController>(
                    $"Error executing command: {input.Command}. Exception: {ex.Message}");
                return (CommandExecutionResult.Error($"Error executing command: {input.Command}"), null);
            }
        }
    }
}