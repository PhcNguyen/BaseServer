using NServer.Core.Packets;

namespace NServer.Application.Handlers.Packets.Queue
{
    /// <summary>
    /// Hàng đợi gói tin dùng để xử lý các gói tin nhận.
    /// </summary>
    public class PacketIncoming : PacketQueueDispatcher
    {
        public PacketIncoming() : base()
        {
        }
    }
}