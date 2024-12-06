using NPServer.Core.Packets.Abstract;

namespace NPServer.Application.Handlers.Packets.Queue
{
    /// <summary>
    /// Hàng đợi gói tin server dùng để xử lý các gói tin gửi.
    /// </summary>
    public class PacketInserver : AbstractPacketQueue
    {
        public PacketInserver() : base()
        {
        }
    }
}