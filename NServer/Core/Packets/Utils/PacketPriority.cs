using System;
using System.Collections.Generic;

using NServer.Core.Packets.Enums;
using NServer.Core.Interfaces.Packets;

namespace NServer.Core.Packets.Utils
{
    /// <summary>
    /// Tiện ích xử lý ưu tiên và cờ trạng thái của gói tin.
    /// </summary>
    internal static class PacketPriority
    {
        /// <summary>
        /// Xác định độ ưu tiên của gói tin dựa trên cờ (flags).
        /// </summary>
        public static int DeterminePriority(IPacket packet) => packet.Flags switch
        {
            PacketFlags f when f.HasFlag(PacketFlags.ISURGENT) => 4, // Khẩn cấp
            PacketFlags f when f.HasFlag(PacketFlags.HIGH) => 3,     // Cao
            PacketFlags f when f.HasFlag(PacketFlags.MEDIUM) => 2,   // Trung bình
            PacketFlags f when f.HasFlag(PacketFlags.LOW) => 1,      // Thấp
            _ => 0,                                                  // Không có cờ ưu tiên nào
        };

        /// <summary>
        /// Lấy danh sách các cờ trạng thái hiện tại của gói tin.
        /// </summary>
        public static IEnumerable<PacketFlags> CombinedFlags(this IPacket packet, Func<PacketFlags, bool>? filter = null)
        {
            for (int i = 0; i < sizeof(PacketFlags) * 8; i++)
            {
                var flag = (PacketFlags)(1 << i);
                if ((packet.Flags & flag) == flag && (filter == null || filter(flag)))
                {
                    yield return flag;
                }
            }
        }
    }
}