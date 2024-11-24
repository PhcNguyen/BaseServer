using System;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using NServer.Core.Packets;
using NServer.Infrastructure.Logging;
using NServer.Core.Interfaces.Session;

namespace NServer.Application.Handler
{
    internal class CommandDispatcher
    {
        private static readonly BindingFlags BindingFlags = (
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
                           .SelectMany(t => t.GetMethods(BindingFlags))
                           .Where(m => m.GetCustomAttribute<CommandAttribute>() != null)
                           .ToDictionary(
                               m => m.GetCustomAttribute<CommandAttribute>()!.Command, // Key: Command từ Attribute
                               m => m // Value: MethodInfo
                           );
        }

        private static Dictionary<Cmd, MethodInfo> CommandCacheValue => CommandCache.Value;

        public static async ValueTask HandleCommand(ISessionClient? session, Packet packet, CancellationToken cancellationToken = default)
        {
            if (session == null)
            {
                NLog.Instance.Error("Session is null.");
                return;
            }

            Packet newPacket = new();
            newPacket.SetCommand((short)Cmd.ERROR);

            if (packet.Command == (short)Cmd.NONE)
            {
                newPacket.SetPayload("Invalid command: Command is null or invalid.");
                await session.Send(newPacket.ToByteArray());
                return;
            }

            var command = (Cmd)packet.Command;

            if (!CommandCacheValue.TryGetValue(command, out var method))
            {
                newPacket.SetPayload($"Unknown command: {command}");
                await session.Send(newPacket.ToByteArray());
                return;
            }

            try
            {
                // Delegate caching (caching delegate to improve performance)
                var func = (Func<ISessionClient, byte[], Task>)method
                            .CreateDelegate(typeof(Func<ISessionClient, byte[], Task>),
                            method.IsStatic ? null : new CommandDispatcher());

                // ToArray only when necessary (this avoids unnecessary allocation if not needed)
                var payloadArray = packet.Payload.Span.ToArray();
                await func(session, payloadArray);
            }
            catch (Exception ex)
            {
                newPacket.SetPayload($"Error executing command: {command}");
                await session.Send(newPacket.ToByteArray());

                NLog.Instance.Error($"Error executing command: {command}. Exception: {ex.Message}");
            }
        }
    }
}