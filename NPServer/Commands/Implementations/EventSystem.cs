using NPServer.Commands.Attributes;
using NPServer.Core.Interfaces.Packets;
using NPServer.Core.Packets.Utilities;
using NPServer.Models.Database;
using System.Threading.Tasks;

namespace NPServer.Commands.Implementations
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
        [Command(Command.Ping, UserRole.Guests)]
        public static Task<IPacket> Ping() =>
            Task.FromResult(((short)Command.Pong).ToResponsePacket("Ping received. Server is responsive."));

        /// <summary>
        /// Phương thức xử lý lệnh pong.
        /// </summary>
        /// <returns>Gói tin phản hồi với thông báo ping.</returns>
        [Command(Command.Pong, UserRole.Guests)]
        public static Task<IPacket> Pong() =>
            Task.FromResult(((short)Command.Ping).ToResponsePacket("Pong received. Server is responsive."));

        /// <summary>
        /// Phương thức xử lý lệnh heartbeat.
        /// </summary>
        /// <returns>Gói tin phản hồi với thông báo thành công và trạng thái sống của server.</returns>
        [Command(Command.Heartbeat, UserRole.Guests)]
        public static Task<IPacket> Heartbeat() =>
            Task.FromResult(((short)Command.Success).ToResponsePacket("Server is alive and operational."));

        /// <summary>
        /// Phương thức xử lý lệnh close.
        /// </summary>
        /// <returns>Gói tin rỗng để đóng kết nối.</returns>
        [Command(Command.Close, UserRole.Guests)]
        public static Task<IPacket> Close() =>
            Task.FromResult(PacketExtensions.EmptyPacket);
    }
}