using NServer.Core.Interfaces.Packets;
using System;
using System.Collections.Generic;

namespace NServer.Core.Packets.Utils;

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

        if ((flags & Enums.PacketFlags.ISURGENT) == Enums.PacketFlags.ISURGENT) return 4; // Khẩn cấp
        if ((flags & Enums.PacketFlags.HIGH) == Enums.PacketFlags.HIGH) return 3;         // Cao
        if ((flags & Enums.PacketFlags.MEDIUM) == Enums.PacketFlags.MEDIUM) return 2;     // Trung bình
        if ((flags & Enums.PacketFlags.LOW) == Enums.PacketFlags.LOW) return 1;           // Thấp

        return 0; // Không có cờ ưu tiên nào
    }

    /// <summary>
    /// Lấy danh sách các cờ trạng thái hiện tại của gói tin.
    /// </summary>
    public static IEnumerable<Enums.PacketFlags> CombinedFlags(this IPacket packet, Func<Enums.PacketFlags, bool>? filter = null)
    {
        for (int i = 0; i < sizeof(Enums.PacketFlags) * 8; i++)
        {
            var flag = (Enums.PacketFlags)(1 << i);
            if ((packet.Flags & flag) == flag && (filter == null || filter(flag)))
            {
                yield return flag;
            }
        }
    }
}