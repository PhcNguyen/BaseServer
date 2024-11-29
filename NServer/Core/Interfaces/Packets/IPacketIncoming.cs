using System;

using NServer.Infrastructure.Services;

namespace NServer.Core.Interfaces.Packets
{
    internal interface IPacketIncoming: IPacketQueue
    {
        event Action? PacketAdded;

        bool AddPacket(UniqueId id, byte[]? packet);
        bool AddPacket(byte[]? packet);
    }
}
