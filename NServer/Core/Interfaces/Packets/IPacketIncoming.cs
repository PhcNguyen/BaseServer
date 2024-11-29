using System;

namespace NServer.Core.Interfaces.Packets
{
    internal interface IPacketIncoming: IPacketQueue
    {
        event Action? PacketAdded;

        bool AddPacket(IPacket packet);
    }
}
