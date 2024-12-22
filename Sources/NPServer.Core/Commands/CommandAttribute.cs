namespace NPServer.Core.Commands;

/// <summary>
/// Attribute để đánh dấu các phương thức xử lý lệnh.
/// </summary>
/// <remarks>
/// Gắn kết một lệnh cụ thể với một phương thức xử lý.
/// </remarks>
/// <remarks>
/// Tạo mới một CommandAttribute với lệnh và vai trò yêu cầu cụ thể.
/// </remarks>
/// <param name="command">Lệnh được liên kết với phương thức.</param>
/// <param name="requiredRole">Vai trò yêu cầu để thực thi lệnh.</param>
[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false)]
public sealed class CommandAttribute(Common.Models.Command command, Common.Models.AccessLevel requiredRole)
    : System.Attribute
{
    /// <summary>
    /// Lệnh được liên kết với phương thức.
    /// </summary>
    public Common.Models.Command Command { get; } = command;

    /// <summary>
    /// Vai trò yêu cầu để thực thi lệnh này.
    /// </summary>
    public Common.Models.AccessLevel RequiredRole { get; } = requiredRole;
}