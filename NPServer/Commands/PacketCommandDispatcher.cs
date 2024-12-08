using NPServer.Models.Database;
using NPServer.Commands.Abstract;
using NPServer.Core.Communication.Utilities;
using NPServer.Core.Interfaces.Communication;
using NPServer.Infrastructure.Logging;
using System;

namespace NPServer.Commands
{
    internal class PacketCommandDispatcher : CommandDispatcherBase
    {
        private static readonly string[] TargetNamespaces =
        [
            "NServer.Application.Handlers.Client",
        ];

        public PacketCommandDispatcher() : base(TargetNamespaces)
        {
        }

        public (IPacket, IPacket?) HandleCommand(IPacket packet, UserRole userRole)
        {
            Command command = (Command)packet.Cmd;

            if (!CommandDelegateCache.TryGetValue(command, out var commandInfo))
                return (((short)Command.Error).ToResponsePacket($"Unknown command: {command}"), null);

            var (requiredRole, func) = commandInfo;

            if (userRole < requiredRole)
            {
                return (((short)Command.Error).ToResponsePacket($"Permission denied for command: {command}"), null);
            }

            try
            {
                // Thực thi lệnh và ép kiểu kết quả về IPacket
                if (func(packet) is not IPacket result)
                    throw new InvalidOperationException("Invalid result type from command handler.");

                return (result, null);
            }
            catch (Exception ex)
            {
                NPLog.Instance.Error<CommandDispatcherBase>($"Error executing command: {command}. Exception: {ex.Message}");
                return ((((short)Command.Error).ToResponsePacket($"Error executing command: {command}"), null));
            }
        }
    }
}