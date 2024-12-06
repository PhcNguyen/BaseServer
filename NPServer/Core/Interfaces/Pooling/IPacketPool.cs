using NPServer.Core.Interfaces.Packets;

namespace NPServer.Core.Interfaces.Pooling
{
    public interface IPacketPool
    {
        int Count { get; }

        IPacket RentPacket();

        void ReturnPacket(IPacket packet);
    }
}