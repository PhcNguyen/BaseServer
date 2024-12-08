using NPServer.Models.Database;
using NPServer.Commands.Attributes;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;


namespace NPServer.Commands.Abstract
{
    /// <summary>
    /// Lớp cơ sở xử lý các lệnh trong hệ thống.
    /// Cung cấp cơ chế đăng ký và thực thi các lệnh dựa trên phương thức.
    /// </summary>
    internal abstract class CommandDispatcherBase
    {
        /// <summary>
        /// Cờ binding cho các phương thức lệnh (bao gồm Public, Static, và Instance).
        /// </summary>
        private readonly BindingFlags CommandBindingFlags =
            BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        /// <summary>
        /// Cache các lệnh đã được đăng ký cùng với phương thức xử lý và vai trò yêu cầu.
        /// </summary>
        protected readonly ConcurrentDictionary<Command, (UserRole RequiredRole, Func<object?, object> Handler)> CommandDelegateCache;

        /// <summary>
        /// Khởi tạo đối tượng xử lý lệnh.
        /// Tải các phương thức lệnh từ các namespace mục tiêu và đăng ký chúng.
        /// </summary>
        /// <param name="targetNamespaces">Danh sách các namespace mà từ đó các phương thức lệnh sẽ được tải.</param>
        protected CommandDispatcherBase(string[] targetNamespaces)
        {
            var commandMethods = LoadCommandMethods(targetNamespaces);
            CommandDelegateCache = new ConcurrentDictionary<Command, (UserRole, Func<object?, object>)>();

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
        private IEnumerable<(Command Command, MethodInfo Method, UserRole RequiredRole)> LoadCommandMethods(string[] targetNamespaces)
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            return assembly.GetTypes()
                .Where(t => targetNamespaces.Contains(t.Namespace))
                .SelectMany(t => t.GetMethods(CommandBindingFlags))
                .Where(m => m.GetCustomAttribute<CommandAttribute>() != null)
                .Select(m =>
                {
                    var attribute = m.GetCustomAttribute<CommandAttribute>() ?? throw new InvalidOperationException($"Method {m.Name} does not have a valid CommandAttribute.");
                    return (attribute.Command, m, attribute.RequiredRole);
                });
        }

        /// <summary>
        /// Đăng ký lệnh với phương thức xử lý và vai trò yêu cầu.
        /// </summary>
        /// <param name="command">Lệnh cần đăng ký.</param>
        /// <param name="method">Phương thức xử lý lệnh.</param>
        /// <param name="requiredRole">Vai trò yêu cầu để thực hiện lệnh.</param>
        private void RegisterCommand(Command command, MethodInfo method, UserRole requiredRole)
        {
            var commandDelegate = CreateDelegate(method);
            CommandDelegateCache[command] = (requiredRole, commandDelegate);
        }

        /// <summary>
        /// Tạo delegate từ phương thức đã cho để thực thi lệnh.
        /// </summary>
        /// <param name="method">Phương thức cần tạo delegate.</param>
        /// <returns>Delegate thực thi phương thức tương ứng.</returns>
        private static Func<object?, object> CreateDelegate(MethodInfo method)
        {
            ArgumentNullException.ThrowIfNull(method);

            // Kiểm tra kiểu trả về của phương thức
            if (method.ReturnType != typeof(object))
                throw new ArgumentException("Method must return object", nameof(method));

            var parameters = method.GetParameters();

            // Trường hợp phương thức không có tham số
            if (parameters.Length == 0)
            {
                return _ =>
                {
                    var result = method.Invoke(null, null);
                    return result ?? throw new InvalidOperationException("Method returned null or an invalid result.");
                };
            }

            // Trường hợp phương thức có một tham số kiểu object
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(object))
            {
                return (parameter) =>
                {
                    object? result = method.Invoke(null, [parameter!]);
                    return result ?? throw new InvalidOperationException("Method returned null or an invalid result.");
                };
            }

            // Nếu phương thức không phù hợp với các trường hợp trên
            throw new ArgumentException("Method signature is invalid. It must either have no parameters or one object parameter.");
        }
    }
}