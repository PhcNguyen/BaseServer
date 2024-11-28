using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Core.Interfaces.Packets
{
    /// <summary>
    /// Giao diện cơ bản cho hàng đợi gói tin.
    /// </summary>
    internal interface IPacketQueue
    {
        IPacket? Dequeue();
        IPacket? Peek();
        List<IPacket> DequeueBatch(int batchSize);
        int Count();
        void Clear();
        bool Contains(IPacket packet);
        IEnumerable<IPacket> Filter(Func<IPacket, bool> predicate);
    }
}
