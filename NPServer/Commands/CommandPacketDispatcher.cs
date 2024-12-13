using NPServer.Infrastructure.Logging;
using NPServer.Models.Common;
using NPServer.Packets;

namespace NPServer.Commands
{
    internal class CommandPacketDispatcher : CommandDispatcher
    {
        private static readonly string[] TargetNamespaces =
        [
            "NServer.Commands.Implementations",
        ];

        public CommandPacketDispatcher() : base(TargetNamespaces)
        {
        }

        public (Packet, Packet?) HandleCommand(Packet packet, AccessLevel userRole)
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
                if (func(packet) is not Packet result)
                    throw new System.InvalidOperationException("Invalid result type from command handler.");

                return (result, null);
            }
            catch (System.Exception ex)
            {
                NPLog.Instance.Error<CommandPacketDispatcher>($"Error executing command: {command}. Exception: {ex.Message}");
                return (((short)Command.Error).ToResponsePacket($"Error executing command: {command}"), null);
            }
        }
    }
}