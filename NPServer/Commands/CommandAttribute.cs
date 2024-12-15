namespace NPServer.Commands;

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
public sealed class CommandAttribute(Models.Common.Command command, Models.Common.AccessLevel requiredRole) 
    : System.Attribute
{
    /// <summary>
    /// Lệnh được liên kết với phương thức.
    /// </summary>
    public Models.Common.Command Command { get; } = command;

    /// <summary>
    /// Vai trò yêu cầu để thực thi lệnh này.
    /// </summary>
    public Models.Common.AccessLevel RequiredRole { get; } = requiredRole;
}