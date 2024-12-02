using NServer.Core.Packets;

namespace NServer.Application.Handlers.Packets.Queue
{
    /// <summary>
    /// Hàng đợi gói tin server dùng để xử lý các gói tin gửi.
    /// </summary>
    public class PacketInserver : PacketQueueDispatcher
    {
        public PacketInserver() : base()
        {
        }
    }
}