using System;
using System.Collections.Generic;

namespace NServer.Core.Interfaces.Packets
{
    internal interface IPacketSender
    {
        public event Action? PacketAdded;

        void AddPacket(IPacket packet);
        public IPacket? DequeuePacket();
        List<IPacket> DequeueBatch(int batchSize);
        int Count();
        void Dispose();
    }
}
