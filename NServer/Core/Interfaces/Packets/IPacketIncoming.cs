using System;

using Base.Infrastructure.Services;

namespace Base.Core.Interfaces.Packets
{
    internal interface IPacketIncoming: IPacketQueue
    {
        event Action? PacketAdded;

        bool AddPacket(UniqueId id, byte[]? packet);
        bool AddPacket(byte[]? packet);
    }
}
