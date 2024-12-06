using NPServer.Core.Interfaces.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NPServer.Core.Handlers
{
    internal abstract class CommandDispatcherBase<TCommand> where TCommand : notnull
    {
        private readonly BindingFlags CommandBindingFlags =
            BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        protected readonly ConcurrentDictionary<TCommand, Func<IPacket?, Task<IPacket>>> CommandDelegateCache;

        protected CommandDispatcherBase(string[] targetNamespaces)
        {
            var commandMethods = this.LoadCommandMethods(targetNamespaces);
            CommandDelegateCache = new ConcurrentDictionary<TCommand, Func<IPacket?, Task<IPacket>>>();

            foreach (var (command, method) in commandMethods)
            {
                RegisterCommand(command, method);
            }
        }

        private Dictionary<TCommand, MethodInfo> LoadCommandMethods(string[] targetNamespaces)
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            return assembly.GetTypes()
                .Where(t => targetNamespaces.Contains(t.Namespace))
                .SelectMany(t => t.GetMethods(this.CommandBindingFlags))
                .Where(m => m.GetCustomAttribute<CommandAttribute<TCommand>>() != null)
                .ToDictionary(
                    m =>
                    {
                        var attribute = m.GetCustomAttribute<CommandAttribute<TCommand>>();
                        if (attribute == null || string.IsNullOrEmpty(attribute.Command.ToString()))
                        {
                            throw new InvalidOperationException($"Method {m.Name} does not have a valid CommandAttribute.");
                        }

                        return (TCommand)Enum.Parse(typeof(TCommand), attribute.Command.ToString()!);
                    },
                    m => m
                );
        }

        private void RegisterCommand(TCommand command, MethodInfo method)
        {
            var commandDelegate = CreateDelegate(method);
            CommandDelegateCache[command] = commandDelegate;
        }

        private static Func<IPacket?, Task<IPacket>> CreateDelegate(MethodInfo method)
        {
            ArgumentNullException.ThrowIfNull(method);

            if (method.ReturnType != typeof(Task<IPacket>))
                throw new ArgumentException("Method must return Task<IPacket>", nameof(method));

            var parameters = method.GetParameters();

            if (parameters.Length == 0)
            {
                return _ =>
                {
                    if (method.Invoke(null, null) is not Task<IPacket> result)
                        throw new InvalidOperationException("Method returned null.");
                    return result;
                };
            }

            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IPacket))
            {
                return (packet) =>
                {
                    if (method.Invoke(null, [packet!]) is not Task<IPacket> result)
                        throw new InvalidOperationException("Method returned null.");
                    return result;
                };
            }

            throw new ArgumentException("Method signature is invalid.");
        }
    }
}