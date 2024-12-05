using NServer.Application.Handlers.Packets;
using NServer.Core.Handlers;
using NServer.Core.Interfaces.Packets;
using System.Threading.Tasks;

namespace NServer.Application.Handlers.Client
{
    /// <summary>
    /// Lớp xử lý các lệnh hệ thống từ phía khách hàng.
    /// <para>
    /// Lớp này cung cấp các phương thức để xử lý các lệnh hệ thống như ping, pong, heartbeat và close từ phía khách hàng.
    /// </para>
    /// </summary>
    internal static class EventSystem
    {
        /// <summary>
        /// Phương thức xử lý lệnh ping.
        /// </summary>
        /// <returns>Gói tin phản hồi với thông báo pong.</returns>
        [CommandAttribute<Command>(Command.PING)]
        public static Task<IPacket> Ping() =>
            Task.FromResult(PacketUtils.Response(Command.PONG, "Ping received. Server is responsive."));

        /// <summary>
        /// Phương thức xử lý lệnh pong.
        /// </summary>
        /// <returns>Gói tin phản hồi với thông báo ping.</returns>
        [CommandAttribute<Command>(Command.PONG)]
        public static Task<IPacket> Pong() =>
            Task.FromResult(PacketUtils.Response(Command.PING, "Pong received. Server is responsive."));

        /// <summary>
        /// Phương thức xử lý lệnh heartbeat.
        /// </summary>
        /// <returns>Gói tin phản hồi với thông báo thành công và trạng thái sống của server.</returns>
        [CommandAttribute<Command>(Command.HEARTBEAT)]
        public static Task<IPacket> Heartbeat() =>
            Task.FromResult(PacketUtils.Response(Command.SUCCESS, "Server is alive and operational."));

        /// <summary>
        /// Phương thức xử lý lệnh close.
        /// </summary>
        /// <returns>Gói tin rỗng để đóng kết nối.</returns>
        [CommandAttribute<Command>(Command.CLOSE)]
        public static Task<IPacket> Close() =>
            Task.FromResult(PacketUtils.EmptyPacket);
    }
}