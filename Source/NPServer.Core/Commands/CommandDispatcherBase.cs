using NPServer.Common.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace NPServer.Core.Commands;

/// <summary>
/// Lớp cơ sở xử lý các lệnh trong hệ thống.
/// Cung cấp cơ chế đăng ký và thực thi các lệnh dựa trên phương thức.
/// </summary>
public abstract class CommandDispatcherBase
{
    /// <summary>
    /// Cờ binding cho các phương thức lệnh (bao gồm Public, Static, và Instance).
    /// </summary>
    private readonly BindingFlags CommandBindingFlags =
        BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

    /// <summary>
    /// Cache các lệnh đã được đăng ký cùng với phương thức xử lý và vai trò yêu cầu.
    /// </summary>
    protected ImmutableDictionary<Command, (Authoritys RequiredRole, Func<object?, object> Handler)> CommandDelegateCache;

    /// <summary>
    /// Khởi tạo đối tượng xử lý lệnh.
    /// Tải các phương thức lệnh từ các namespace mục tiêu và đăng ký chúng.
    /// </summary>
    /// <param name="targetNamespaces">Danh sách các namespace mà từ đó các phương thức lệnh sẽ được tải.</param>
    protected CommandDispatcherBase(string[] targetNamespaces)
    {
        var commandMethods = LoadCommandMethods(targetNamespaces);

        // Chuyển danh sách lệnh thành ImmutableDictionary
        CommandDelegateCache = commandMethods
            .Select(cmd => new KeyValuePair<Command, (Authoritys, Func<object?, object>)>(
                cmd.Command,
                (cmd.RequiredRole, CreateDelegate(cmd.Method))
            ))
            .ToImmutableDictionary();
    }

    /// <summary>
    /// Tải các phương thức lệnh từ assembly hiện tại dựa trên các namespace mục tiêu.
    /// </summary>
    /// <param name="targetNamespaces">Danh sách các namespace mục tiêu.</param>
    /// <returns>Danh sách các lệnh, phương thức, và vai trò yêu cầu tương ứng.</returns>
    private IEnumerable<(Command Command, MethodInfo Method, Authoritys RequiredRole)> LoadCommandMethods(string[] targetNamespaces)
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
    protected void RegisterCommand(Command command, MethodInfo method, Authoritys requiredRole)
    {
        var commandDelegate = CreateDelegate(method);

        // Thêm lệnh mới bằng cách tạo một dictionary bất biến mới
        CommandDelegateCache = CommandDelegateCache.Add(command, (requiredRole, commandDelegate));
    }

    /// <summary>
    /// Hủy đăng ký lệnh.
    /// </summary>
    /// <param name="command">Lệnh cần hủy đăng ký.</param>
    protected void UnregisterCommand(Command command)
    {
        // Xóa lệnh bằng cách tạo dictionary bất biến mới
        CommandDelegateCache = CommandDelegateCache.Remove(command);
    }

    /// <summary>
    /// Tạo delegate từ phương thức đã cho để thực thi lệnh.
    /// </summary>
    /// <param name="method">Phương thức cần tạo delegate.</param>
    /// <returns>Delegate thực thi phương thức tương ứng.</returns>
    private static Func<object?, object> CreateDelegate(MethodInfo method)
    {
        ArgumentNullException.ThrowIfNull(method);

        if (method.ReturnType != typeof(object))
            throw new ArgumentException("Method must return object", nameof(method));

        var parameters = method.GetParameters();

        if (parameters.Length == 0)
        {
            return _ =>
            {
                var result = method.Invoke(null, null);
                return result ?? throw new InvalidOperationException("Method returned null or an invalid result.");
            };
        }

        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(object))
        {
            return (parameter) =>
            {
                var result = method.Invoke(null, [parameter!]);
                return result ?? throw new InvalidOperationException("Method returned null or an invalid result.");
            };
        }

        throw new ArgumentException("Method signature is invalid. It must either have no parameters or one object parameter.");
    }
}