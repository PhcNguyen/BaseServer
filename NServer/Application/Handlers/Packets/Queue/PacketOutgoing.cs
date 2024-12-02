using NServer.Core.Packets;

namespace NServer.Application.Handlers.Packets.Queue
{
    /// <summary>
    /// Hàng đợi gói tin dùng để xử lý các gói tin gửi.
    /// </summary>
    public class PacketOutgoing : PacketQueueDispatcher
    {
        public PacketOutgoing() : base()
        {
        }
    }
}