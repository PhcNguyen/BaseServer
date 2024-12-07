using NPServer.Models.Database;
using System;

namespace NPServer.Commands.Attributes
{
    /// <summary>
    /// Attribute để đánh dấu các phương thức xử lý lệnh.
    /// </summary>
    /// <remarks>
    /// Tạo một attribute với lệnh cụ thể.
    /// </remarks>
    /// <param name="command">Lệnh liên kết với phương thức.</param>
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute(Command command, UserRole requiredRole) : Attribute
    {
        /// <summary>
        /// Lệnh được liên kết với phương thức.
        /// </summary>
        public Command Command { get; } = command;
        public UserRole RequiredRole { get; } = requiredRole;
    }

    /// <summary>
    /// Indicates that a method is the default command for the <see cref="CommandGroup"/> it belongs to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DefaultCommandAttribute(UserRole requiredRole) : CommandAttribute(Command.Default, requiredRole)
    {
    }
}