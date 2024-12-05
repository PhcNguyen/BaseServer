using NServer.Core.Interfaces.Packets;

namespace NServer.Core.Interfaces.Pooling
{
    public interface IPacketPool
    {
        int Count { get; }

        IPacket RentPacket();

        void ReturnPacket(IPacket packet);
    }
}
