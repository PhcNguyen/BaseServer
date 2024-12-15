using NPServer.Commands.Utils;
using NPServer.Models.Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace NPServer.Commands.Abstract;

/// <summary>
/// Lớp cơ sở xử lý các lệnh trong hệ thống.
/// Cung cấp cơ chế đăng ký và thực thi các lệnh dựa trên phương thức.
/// </summary>
internal abstract class AbstractCommandDispatcher
{
    /// <summary>
    /// Cờ binding cho các phương thức lệnh (bao gồm Public, Static, và Instance).
    /// </summary>
    private readonly BindingFlags CommandBindingFlags =
        BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

    /// <summary>
    /// Cache các lệnh đã được đăng ký cùng với phương thức xử lý và vai trò yêu cầu.
    /// </summary>
    protected ImmutableDictionary<Command, (AccessLevel RequiredRole, Func<object?, object> Handler)> CommandDelegateCache;

    /// <summary>
    /// Khởi tạo đối tượng xử lý lệnh.
    /// Tải các phương thức lệnh từ các namespace mục tiêu và đăng ký chúng.
    /// </summary>
    /// <param name="targetNamespaces">Danh sách các namespace mà từ đó các phương thức lệnh sẽ được tải.</param>
    protected AbstractCommandDispatcher(string[] targetNamespaces)
    {
        var commandMethods = LoadCommandMethods(targetNamespaces);

        // Chuyển danh sách lệnh thành ImmutableDictionary
        CommandDelegateCache = commandMethods
            .Select(cmd => new KeyValuePair<Command, (AccessLevel, Func<object?, object>)>(
                cmd.Command,
                (cmd.RequiredRole, CommandMethodHandler.CreateDelegate(cmd.Method))
            ))
            .ToImmutableDictionary();
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
    protected void RegisterCommand(Command command, MethodInfo method, AccessLevel requiredRole)
    {
        var commandDelegate = CommandMethodHandler.CreateDelegate(method);

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
}