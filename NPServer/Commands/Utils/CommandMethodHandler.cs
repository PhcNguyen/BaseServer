using System;
using System.Reflection;

namespace NPServer.Commands.Utils;

public static class CommandMethodHandler
{
    /// <summary>
    /// Tạo delegate từ phương thức đã cho để thực thi lệnh.
    /// </summary>
    /// <param name="method">Phương thức cần tạo delegate.</param>
    /// <returns>Delegate thực thi phương thức tương ứng.</returns>
    public static Func<object?, object> CreateDelegate(MethodInfo method)
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