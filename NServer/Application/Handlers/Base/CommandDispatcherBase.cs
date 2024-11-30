using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using NServer.Core.Interfaces.Packets;
using NServer.Infrastructure.Logging;
using NServer.Application.Handlers.Packets;

namespace NServer.Application.Handlers.Base
{
    /// <summary>
    /// Lớp cơ sở để quản lý việc phân phối các lệnh đến các phương thức xử lý tương ứng.
    /// </summary>
    internal abstract class CommandDispatcherBase
    {
        private static readonly BindingFlags CommandBindingFlags = (
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance
        );

        protected readonly ConcurrentDictionary<Cmd, MethodInfo> CommandCache;
        protected readonly ConcurrentDictionary<Cmd, Lazy<Func<Task<IPacket>>>> CommandDelegateCacheNoPayload;
        protected readonly ConcurrentDictionary<Cmd, Lazy<Func<IPacket, Task<IPacket>>>> CommandDelegateCacheWithPayload;

        /// <summary>
        /// Khởi tạo một đối tượng <see cref="CommandDispatcherBase"/> mới.
        /// </summary>
        /// <param name="targetNamespaces">Danh sách các namespace cần tìm kiếm phương thức lệnh.</param>
        protected CommandDispatcherBase(string[] targetNamespaces)
        {
            CommandCache = LoadMethodsWithCommandAttribute(targetNamespaces);

            CommandDelegateCacheNoPayload = new ConcurrentDictionary<Cmd, Lazy<Func<Task<IPacket>>>>();
            CommandDelegateCacheWithPayload = new ConcurrentDictionary<Cmd, Lazy<Func<IPacket, Task<IPacket>>>>();

            foreach (KeyValuePair<Cmd, MethodInfo> command in CommandCache)
            {
                MethodInfo method = command.Value;

                // Đăng ký phương thức
                RegisterMethod(command.Key, method);
            }
        }

        /// <summary>
        /// Xử lý lệnh từ gói tin.
        /// </summary>
        /// <param name="packet">Gói tin chứa lệnh cần xử lý.</param>
        /// <returns>Kết quả xử lý gói tin.</returns>
        public async Task<IPacket> HandleCommand(IPacket packet)
        {
            Cmd command = (Cmd)packet.Cmd;

            if (!CommandCache.TryGetValue(command, out MethodInfo? method))
                return PacketUtils.Response(Cmd.ERROR, $"Unknown command: {command}");

            try
            {
                if (CommandDelegateCacheWithPayload.TryGetValue(command, out Lazy<Func<IPacket, Task<IPacket>>>? funcWithPayload))
                {
                    IPacket result = await funcWithPayload.Value(packet);
                    result.SetId(packet.Id);
                    return result;
                }

                if (CommandDelegateCacheNoPayload.TryGetValue(command, out Lazy<Func<Task<IPacket>>>? funcNoPayload))
                {
                    IPacket result = await funcNoPayload.Value();
                    result.SetId(packet.Id);
                    return result;
                }

                return PacketUtils.Response(Cmd.ERROR, $"Unknown command: {command}");
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Error executing command: {command}. Exception: {ex.Message}");
                return PacketUtils.Response(Cmd.ERROR, $"Error executing command: {command}");
            }
        }

        /// <summary>
        /// Tải các phương thức có thuộc tính CommandAttribute từ các namespace mục tiêu.
        /// </summary>
        /// <param name="targetNamespaces">Danh sách các namespace cần tìm kiếm phương thức lệnh.</param>
        /// <returns>Từ điển chứa các cặp lệnh và phương thức tương ứng.</returns>
        private static ConcurrentDictionary<Cmd, MethodInfo> LoadMethodsWithCommandAttribute(string[] targetNamespaces)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Lấy tất cả các phương thức có CommandAttribute
            IEnumerable<MethodInfo> methods = assembly.GetTypes()
                                                      .Where(t => targetNamespaces.Contains(t.Namespace))
                                                      .SelectMany(t => t.GetMethods(CommandBindingFlags))
                                                      .Where(m => m.GetCustomAttribute<CommandAttribute>() != null);

            // Chuyển sang dictionary
            return new ConcurrentDictionary<Cmd, MethodInfo>(
                methods.ToDictionary(
                    m => m.GetCustomAttribute<CommandAttribute>()!.Command,
                    m => m
                )
            );
        }

        /// <summary>
        /// Đăng ký phương thức vào cache.
        /// </summary>
        /// <param name="command">Lệnh cần đăng ký.</param>
        /// <param name="method">Phương thức cần đăng ký.</param>
        private void RegisterMethod(Cmd command, MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            // Kiểm tra nếu phương thức không có tham số
            if (parameters.Length == 0)
            {
                RegisterCommand(CommandDelegateCacheNoPayload, command, () => CreateCommandDelegate(method));
            }
            // Kiểm tra nếu phương thức có tham số IPacket
            else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IPacket))
            {
                RegisterCommand(CommandDelegateCacheWithPayload, command, () => CreatePacketCommandDelegate(method));
            }
            else
            {
                NLog.Instance.Error($"The method for {command} command is invalid.");
                throw new ArgumentException($"The method for {command} command is invalid.");
            }
        }

        /// <summary>
        /// Tạo delegate cho phương thức lệnh không có tham số.
        /// </summary>
        /// <param name="method">Phương thức cần tạo delegate.</param>
        /// <returns>Delegate cho phương thức lệnh không có tham số.</returns>
        private static Func<Task<IPacket>> CreateCommandDelegate(MethodInfo method)
        {
            ArgumentNullException.ThrowIfNull(method);

            if (method.ReturnType != typeof(Task<IPacket>))
                throw new ArgumentException("Method to return to Task<IPacket>", nameof(method));

            if (method.GetParameters().Length != 0)
                throw new ArgumentException("Methods must not have parameters", nameof(method));

            return () =>
            {
                // Gọi phương thức và trả về Task<IPacket>
                if (method.Invoke(null, null) is not Task<IPacket> result)
                    throw new InvalidOperationException("Null return method, Task request<IPacket>.");

                return result;
            };
        }

        /// <summary>
        /// Tạo delegate cho phương thức lệnh có tham số IPacket.
        /// </summary>
        /// <param name="method">Phương thức cần tạo delegate.</param>
        /// <returns>Delegate cho phương thức lệnh có tham số IPacket.</returns>
        private static Func<IPacket, Task<IPacket>> CreatePacketCommandDelegate(MethodInfo method)
        {
            ArgumentNullException.ThrowIfNull(method);

            if (method.ReturnType != typeof(Task<IPacket>))
                throw new ArgumentException("Method to return to Task<IPacket>", nameof(method));

            ParameterInfo[] parameters = method.GetParameters();

            // Kiểm tra nếu phương thức có 1 tham số kiểu IPacket
            if (parameters.Length != 1 || parameters[0].ParameterType != typeof(IPacket))
                throw new ArgumentException("The method must have an IPacket-type parameter", nameof(method));

            return (IPacket packet) =>
            {
                // Gọi phương thức với tham số IPacket và trả về Task<IPacket>
                if (method.Invoke(null, [packet]) is not Task<IPacket> result)
                    throw new InvalidOperationException("Null return method, Task request<IPacket>.");

                return result;
            };
        }

        /// <summary>
        /// Hàm tiện ích đăng ký lệnh vào cache.
        /// </summary>
        /// <typeparam name="T">Loại delegate.</typeparam>
        /// <param name="commandDelegateCache">Cache lệnh.</typeparam>
        /// <param name="command">Lệnh cần đăng ký.</param>
        /// <param name="createDelegate">Hàm tạo delegate.</param>
        private static void RegisterCommand<T>(ConcurrentDictionary<Cmd, Lazy<T>> commandDelegateCache, Cmd command, Func<T> createDelegate)
        {
            commandDelegateCache[command] = new Lazy<T>(createDelegate);
        }
    }
}