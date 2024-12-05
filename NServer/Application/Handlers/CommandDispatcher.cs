using NServer.Application.Handlers.Packets;
using NServer.Core.Handlers;
using NServer.Core.Interfaces.Packets;
using NServer.Infrastructure.Logging;
using System;
using System.Threading.Tasks;

namespace NServer.Application.Handlers
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
                return PacketUtils.Response(Command.ERROR, $"Unknown command: {command}");

            try
            {
                IPacket result = await func(packet);
                result.SetId(packet.Id);
                return result;
            }
            catch (Exception ex)
            {
                NLog.Instance.Error<CommandDispatcher>($"Error executing command: {command}. Exception: {ex.Message}");
                return PacketUtils.Response(Command.ERROR, $"Error executing command: {command}");
            }
        }
    }
}