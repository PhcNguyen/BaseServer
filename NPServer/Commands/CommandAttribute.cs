using NPServer.Models.Cmd;
using NPServer.Models.Common;
using System;

namespace NPServer.Commands
{
    /// <summary>
    /// Attribute để đánh dấu các phương thức xử lý lệnh.
    /// </summary>
    /// <remarks>
    /// Tạo một attribute với lệnh cụ thể.
    /// </remarks>
    /// <param name="command">Lệnh liên kết với phương thức.</param>
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute(Command command, AccessLevel requiredRole) : Attribute
    {
        /// <summary>
        /// Lệnh được liên kết với phương thức.
        /// </summary>
        public Command Command { get; } = command;
        public AccessLevel RequiredRole { get; } = requiredRole;
    }

    /// <summary>
    /// Indicates that a method is the default command for the <see cref="Command"/> it belongs to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DefaultCommandAttribute(AccessLevel requiredRole) : CommandAttribute(Command.Default, requiredRole)
    {
    }
}