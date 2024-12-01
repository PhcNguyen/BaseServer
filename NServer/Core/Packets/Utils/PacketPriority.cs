using NServer.Core.Interfaces.Packets;
using System;
using System.Collections.Generic;

namespace NServer.Core.Packets.Utils
{
    /// <summary>
    /// Tiện ích xử lý ưu tiên và cờ trạng thái của gói tin.
    /// </summary>
    public static class PacketPriority
    {
        /// <summary>
        /// Xác định độ ưu tiên của gói tin dựa trên cờ (flags).
        /// </summary>
        public static int DeterminePriority(IPacket packet)
        {
            var flags = packet.Flags;

            if ((flags & Enums.Packet.ISURGENT) == Enums.Packet.ISURGENT) return 4; // Khẩn cấp
            if ((flags & Enums.Packet.HIGH) == Enums.Packet.HIGH) return 3;         // Cao
            if ((flags & Enums.Packet.MEDIUM) == Enums.Packet.MEDIUM) return 2;     // Trung bình
            if ((flags & Enums.Packet.LOW) == Enums.Packet.LOW) return 1;           // Thấp

            return 0; // Không có cờ ưu tiên nào
        }


        /// <summary>
        /// Lấy danh sách các cờ trạng thái hiện tại của gói tin.
        /// </summary>
        public static IEnumerable<Enums.Packet> CombinedFlags(this IPacket packet, Func<Enums.Packet, bool>? filter = null)
        {
            for (int i = 0; i < sizeof(Enums.Packet) * 8; i++)
            {
                var flag = (Enums.Packet)(1 << i);
                if ((packet.Flags & flag) == flag && (filter == null || filter(flag)))
                {
                    yield return flag;
                }
            }
        }
    }
}