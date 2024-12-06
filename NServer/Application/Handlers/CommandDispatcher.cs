using NPServer.Application.Handlers.Packets;
using NPServer.Core.Handlers;
using NPServer.Core.Interfaces.Packets;
using NPServer.Core.Packets.Utilities;
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
                return PacketExtensions.ToResponsePacket((short)Command.ERROR, $"Unknown command: {command}");

            try
            {
                IPacket result = await func(packet);
                result.SetId(packet.Id);
                return result;
            }
            catch (Exception ex)
            {
                NLog.Instance.Error<CommandDispatcher>($"Error executing command: {command}. Exception: {ex.Message}");
                return PacketExtensions.ToResponsePacket((short)Command.ERROR, $"Error executing command: {command}");
            }
        }
    }
}