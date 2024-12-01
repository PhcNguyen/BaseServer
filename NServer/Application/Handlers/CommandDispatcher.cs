using NServer.Application.Handlers.Base;

namespace NServer.Application.Handlers
{
    /// <summary>
    /// Lớp xử lý các lệnh từ client, kế thừa từ lớp CommandDispatcherBase.
    /// </summary>
    internal class CommandDispatcher : CommandDispatcherBase
    {
        private static readonly string[] TargetNamespaces =
        [
            "NServer.Application.Handlers.Client",
        ];

        /// <summary>
        /// Khởi tạo một đối tượng <see cref="CommandDispatcher"/> mới.
        /// </summary>
        public CommandDispatcher() : base(TargetNamespaces) { }
    }
}