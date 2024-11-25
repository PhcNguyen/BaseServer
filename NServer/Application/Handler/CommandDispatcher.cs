using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using NServer.Core.Packets;
using NServer.Infrastructure.Logging;

namespace NServer.Application.Handler
{
    internal class CommandDispatcher
    {
        private static readonly BindingFlags BindingFlags = (
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance
        );

        private static readonly Lazy<Dictionary<Cmd, MethodInfo>> CommandCache = new(() =>
            LoadMethodsWithCommandAttribute(
            [
                "NServer.Application.Handler.Client",
                "NServer.Application.Handler.Server"
            ])
        );

        private static readonly Dictionary<Cmd, Func<byte[], Task<Packet>>> CommandDelegateCache = new();

        private static Dictionary<Cmd, MethodInfo> CommandCacheValue => CommandCache.Value;

        private static Dictionary<Cmd, Func<byte[], Task<Packet>>> CommandDelegateCacheValue => CommandDelegateCache;

        private static Dictionary<Cmd, MethodInfo> LoadMethodsWithCommandAttribute(string[] targetNamespaces)
        {
            var assembly = Assembly.GetExecutingAssembly();

            return assembly.GetTypes()
                           .Where(t => targetNamespaces.Contains(t.Namespace))
                           .SelectMany(t => t.GetMethods(BindingFlags))
                           .Where(m => m.GetCustomAttribute<CommandAttribute>() != null)
                           .ToDictionary(
                               m => m.GetCustomAttribute<CommandAttribute>()!.Command,
                               m => m
                           );
        }

        public static async Task<Packet> HandleCommand(Packet packet)
        {
            var newPacket = new Packet();
            newPacket.SetID(packet.Id);
            newPacket.SetCommand((short)Cmd.ERROR);

            if (packet.Command == (short)Cmd.NONE)
            {
                newPacket.SetPayload("Invalid command: Command is null or invalid.");
                return newPacket;
            }

            var command = (Cmd)packet.Command;

            if (!CommandCacheValue.TryGetValue(command, out var method))
            {
                newPacket.SetPayload($"Unknown command: {command}");
                return newPacket;
            }

            try
            {
                // Kiểm tra nếu delegate đã được cache
                if (!CommandDelegateCacheValue.TryGetValue(command, out var func))
                {
                    NLog.Instance.Info($"Creating delegate for command: {command}");
                    func = CreateCommandDelegate(method);
                    CommandDelegateCache[command] = func;
                }

                // Gọi phương thức qua delegate
                var payloadArray = packet.Payload.Span.ToArray();
                var result = await func(payloadArray);

                result.SetID(packet.Id);
                return result;
            }
            catch (Exception ex)
            {
                newPacket.SetPayload($"Error executing command: {command}");
                NLog.Instance.Error($"Error executing command: {command}. Exception: {ex.Message}");
                return newPacket;
            }
        }

        private static Func<byte[], Task<Packet>> CreateCommandDelegate(MethodInfo method)
        {
            // Kiểm tra kiểu trả về của phương thức có phải là Task<Packet>
            if (method.ReturnType != typeof(Task<Packet>))
            {
                throw new ArgumentException("Method must return Task<Packet>", nameof(method));
            }

            // Kiểm tra kiểu tham số đầu vào có phải là byte[]
            var parameters = method.GetParameters();
            if (parameters.Length != 1 || parameters[0].ParameterType != typeof(byte[]))
            {
                throw new ArgumentException("Method must have a single parameter of type byte[]", nameof(method));
            }

            // Tạo delegate với kiểu trả về Task<Packet>
            return (Func<byte[], Task<Packet>>)method.CreateDelegate(
                typeof(Func<byte[], Task<Packet>>),
                method.IsStatic ? null : new CommandDispatcher()
            );
        }

    }
}