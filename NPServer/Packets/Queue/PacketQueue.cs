using NPServer.Infrastructure.Collections;

namespace NPServer.Packets.Queue
{
    public class PacketQueue : CustomQueues<Packet>
    {
        public PacketQueue() : base()
        {
        }
    }
}