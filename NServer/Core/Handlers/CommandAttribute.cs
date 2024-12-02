namespace NServer.Core.Handlers
{
    /// <summary>
    /// Attribute để đánh dấu các phương thức xử lý lệnh.
    /// </summary>
    /// <typeparam name="TCommand">Loại của lệnh.</typeparam>
    /// <remarks>
    /// Tạo một attribute với lệnh cụ thể.
    /// </remarks>
    /// <param name="command">Lệnh liên kết với phương thức.</param>
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class CommandAttribute<TCommand>(TCommand command) : System.Attribute where TCommand : notnull
    {
        /// <summary>
        /// Lệnh được liên kết với phương thức.
        /// </summary>
        public TCommand Command { get; } = command;
    }
}