using NServer.Core.Packet;
using NServer.Infrastructure.Logging;
using NServer.Interfaces.Core.Network;

using System.Reflection;

namespace NServer.Application.Handler
{
    internal class CommandHandler
    {
        private static readonly BindingFlags _bindingFlags = (
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance
        );

        private static readonly Lazy<Dictionary<Cmd, MethodInfo>> CommandCache = new(() =>
            LoadMethodsWithCommandAttribute([
                "NServer.Application.Handler.Client",
                "NServer.Application.Handler.Server"
            ])
        );

        private static Dictionary<Cmd, MethodInfo> LoadMethodsWithCommandAttribute(string[] targetNamespaces)
        {
            var assembly = Assembly.GetExecutingAssembly();

            return assembly.GetTypes()
                           .Where(t => targetNamespaces.Contains(t.Namespace)) // Kiểm tra xem namespace của loại có nằm trong danh sách không
                           .SelectMany(t => t.GetMethods(_bindingFlags))
                           .Where(m => m.GetCustomAttribute<CommandAttribute>() != null)
                           .ToDictionary(
                               m => m.GetCustomAttribute<CommandAttribute>()!.Command, // Key: Command từ Attribute
                               m => m // Value: MethodInfo
                           );
        }

        private static Dictionary<Cmd, MethodInfo> CommandCacheValue => CommandCache.Value;

        public async ValueTask HandleCommand(ISession? session, Packets packet, CancellationToken cancellationToken)
        {
            if (session == null)
            {
                NLog.Error("Session is null.");
                return;
            }

            Packets newPacket = new();
            newPacket.SetCommand((short)Cmd.ERROR);

            if (packet.Command == (short)Cmd.NONE)
            {
                newPacket.SetPayload("Invalid command: Command is null or invalid.");
                session.Send(newPacket.ToByteArray());
                return;
            }

            var command = (Cmd)packet.Command;

            if (!CommandCacheValue.TryGetValue(command, out var method))
            {
                newPacket.SetPayload($"Unknown command: {command}");
                session.Send(newPacket.ToByteArray());
                return;
            }

            try
            {
                // Delegate caching (caching delegate to improve performance)
                var func = (Func<ISession, byte[], CancellationToken, ValueTask>)method
                                .CreateDelegate(typeof(Func<ISession, byte[], CancellationToken, ValueTask>),
                                method.IsStatic ? null : this);

                // ToArray only when necessary (this avoids unnecessary allocation if not needed)
                var payloadArray = packet.Payload.Span.ToArray();
                await func(session, payloadArray, cancellationToken);
            }
            catch (Exception ex)
            {
                newPacket.SetPayload($"Error executing command: {command}");
                session.Send(newPacket.ToByteArray());

                NLog.Error($"Error executing command: {command}. Exception: {ex.Message}");
            }
        }
    }
}