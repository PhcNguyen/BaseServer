using System;
using System.Collections.Generic;

using NServer.Infrastructure.Services;

namespace NServer.Core.Interfaces.Packets
{
    internal interface IPacketReceiver
    {
        public event Action? PacketAdded;

        bool AddPacket(UniqueId id, byte[]? packet);
        bool AddPacket(byte[]? packet);
        public IPacket? DequeuePacket();
        List<IPacket> DequeueBatch(int batchSize);
        int Count();
        void Dispose();
    }
}
