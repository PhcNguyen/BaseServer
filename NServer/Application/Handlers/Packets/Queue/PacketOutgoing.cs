using NPServer.Core.Packets;

namespace NPServer.Application.Handlers.Packets.Queue
{
    /// <summary>
    /// Hàng đợi gói tin dùng để xử lý các gói tin gửi.
    /// </summary>
    public class PacketOutgoing : AbstractPacketQueue
    {
        public PacketOutgoing() : base()
        {
        }
    }
}