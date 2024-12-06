using NPServer.Core.Packets.Abstract;

namespace NPServer.Application.Handlers.Packets.Queue
{
    /// <summary>
    /// Hàng đợi gói tin dùng để xử lý các gói tin nhận.
    /// </summary>
    public class PacketIncoming : AbstractPacketQueue
    {
        public PacketIncoming() : base()
        {
        }
    }
}