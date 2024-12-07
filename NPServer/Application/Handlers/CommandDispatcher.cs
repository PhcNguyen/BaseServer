using NPServer.Core.Handlers;
using NPServer.Core.Packets.Utilities;
using NPServer.Core.Interfaces.Packets;
using NPServer.Infrastructure.Logging;
using System;
using System.Threading.Tasks;

namespace NPServer.Application.Handlers
{
    internal class CommandDispatcher : CommandDispatcherBase<Command>
    {
        private static readonly string[] TargetNamespaces =
        [
            "NServer.Application.Handlers.Client",
        ];

        public CommandDispatcher() : base(TargetNamespaces)
        {
        }

        public async Task<IPacket> HandleCommand(IPacket packet)
        {
            Command command = (Command)packet.Cmd;

            if (!CommandDelegateCache.TryGetValue(command, out var func))
                return ((short)Command.ERROR).ToResponsePacket($"Unknown command: {command}");

            try
            {
                IPacket result = await func(packet);
                result.SetId(packet.Id);
                return result;
            }
            catch (Exception ex)
            {
                NPLog.Instance.Error<CommandDispatcher>($"Error executing command: {command}. Exception: {ex.Message}");
                return ((short)Command.ERROR).ToResponsePacket($"Error executing command: {command}");
            }
        }
    }
}