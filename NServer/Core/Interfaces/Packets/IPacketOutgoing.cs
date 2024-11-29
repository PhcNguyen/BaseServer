using System;

namespace NServer.Core.Interfaces.Packets
{
    internal interface IPacketOutgoing: IPacketQueue
    {
        event Action? PacketAdded;

        void AddPacket(IPacket packet);
    }
}
