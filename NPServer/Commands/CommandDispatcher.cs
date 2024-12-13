using NPServer.Commands.Utils;
using NPServer.Models.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NPServer.Commands
{
    /// <summary>
    /// Lớp cơ sở xử lý các lệnh trong hệ thống.
    /// Cung cấp cơ chế đăng ký và thực thi các lệnh dựa trên phương thức.
    /// </summary>
    internal abstract class CommandDispatcher
    {
        /// <summary>
        /// Cờ binding cho các phương thức lệnh (bao gồm Public, Static, và Instance).
        /// </summary>
        private readonly BindingFlags CommandBindingFlags =
            BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        /// <summary>
        /// Cache các lệnh đã được đăng ký cùng với phương thức xử lý và vai trò yêu cầu.
        /// </summary>
        protected readonly ConcurrentDictionary<Command, (AccessLevel RequiredRole, Func<object?, object> Handler)> CommandDelegateCache;

        /// <summary>
        /// Khởi tạo đối tượng xử lý lệnh.
        /// Tải các phương thức lệnh từ các namespace mục tiêu và đăng ký chúng.
        /// </summary>
        /// <param name="targetNamespaces">Danh sách các namespace mà từ đó các phương thức lệnh sẽ được tải.</param>
        protected CommandDispatcher(string[] targetNamespaces)
        {
            var commandMethods = LoadCommandMethods(targetNamespaces);
            CommandDelegateCache = new ConcurrentDictionary<Command, (AccessLevel, Func<object?, object>)>();

            foreach (var (command, method, requiredRole) in commandMethods)
            {
                RegisterCommand(command, method, requiredRole);
            }
        }

        /// <summary>
        /// Tải các phương thức lệnh từ assembly hiện tại dựa trên các namespace mục tiêu.
        /// </summary>
        /// <param name="targetNamespaces">Danh sách các namespace mục tiêu.</param>
        /// <returns>Danh sách các lệnh, phương thức, và vai trò yêu cầu tương ứng.</returns>
        private IEnumerable<(Command Command, MethodInfo Method, AccessLevel RequiredRole)> LoadCommandMethods(string[] targetNamespaces)
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            return assembly.GetTypes()
                .Where(t => targetNamespaces.Contains(t.Namespace))
                .SelectMany(t => t.GetMethods(CommandBindingFlags))
                .Where(m => m.GetCustomAttribute<CommandAttribute>() != null)
                .Select(m =>
                {
                    var attribute = m.GetCustomAttribute<CommandAttribute>()
                    ?? throw new InvalidOperationException($"Method {m.Name} does not have a valid CommandAttribute.");
                    return (attribute.Command, m, attribute.RequiredRole);
                });
        }

        /// <summary>
        /// Đăng ký lệnh với phương thức xử lý và vai trò yêu cầu.
        /// </summary>
        /// <param name="command">Lệnh cần đăng ký.</param>
        /// <param name="method">Phương thức xử lý lệnh.</param>
        /// <param name="requiredRole">Vai trò yêu cầu để thực hiện lệnh.</param>
        private void RegisterCommand(Command command, MethodInfo method, AccessLevel requiredRole)
        {
            var commandDelegate = MethodHandler.CreateDelegate(method);
            CommandDelegateCache[command] = (requiredRole, commandDelegate);
        }
    }
}