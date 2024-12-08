using NPServer.Core.Interfaces.Communication;

namespace NPServer.Core.Interfaces.Pooling
{
    public interface IPacketPool
    {
        int Count { get; }

        IPacket RentPacket();

        void ReturnPacket(IPacket packet);
    }
}