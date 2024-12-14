using NPServer.Infrastructure.Collections;

namespace NPServer.Application.Handlers.Packets.Queue;

public class PacketQueue : CustomQueues<Packet>
{
    public PacketQueue() : base()
    {
    }
}