using System;

namespace Base.Core.Interfaces.Packets
{
    internal interface IPacketOutgoing: IPacketQueue
    {
        event Action? PacketAdded;

        void AddPacket(IPacket packet);
    }
}
