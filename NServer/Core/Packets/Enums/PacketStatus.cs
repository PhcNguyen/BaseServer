using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NServer.Core.Packets.Enums
{
    internal enum PacketStatus
    {
        CREATED,
        SENT,
        ACKNOWLEDGED,
        ERROR
    }
}
