using NPServer.Core.Packets.Metadata;
using NPServer.Core.Interfaces.Packets;
using System;
using System.Collections.Generic;

namespace NPServer.Core.Packets.Helpers;

/// <summary>
/// Tiện ích xử lý cờ trạng thái và độ ưu tiên của gói tin.
/// </summary>
public static class PacketFlagsHelper
{
    /// <summary>
    /// Enum định nghĩa các mức độ ưu tiên.
    /// </summary>
    public enum PriorityLevel
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        Urgent = 4
    }

    /// <summary>
    /// Xác định độ ưu tiên của gói tin dựa trên cờ (flags).
    /// </summary>
    public static PriorityLevel GetPriority(IPacket packet)
    {
        var flags = packet.Flags;

        if ((flags & PacketFlags.URGENT) == PacketFlags.URGENT)
            return PriorityLevel.Urgent;
        if ((flags & PacketFlags.HIGH) == PacketFlags.HIGH)
            return PriorityLevel.High;
        if ((flags & PacketFlags.MEDIUM) == PacketFlags.MEDIUM)
            return PriorityLevel.Medium;
        if ((flags & PacketFlags.LOW) == PacketFlags.LOW)
            return PriorityLevel.Low;

        return PriorityLevel.None;
    }

    /// <summary>
    /// Lấy danh sách các cờ trạng thái hiện tại của gói tin.
    /// </summary>
    /// <param name="packet">Gói tin cần phân tích.</param>
    /// <param name="filter">Hàm lọc (tùy chọn).</param>
    /// <returns>Các cờ trạng thái được kích hoạt.</returns>
    public static IEnumerable<PacketFlags> GetActiveFlags(this IPacket packet, Func<PacketFlags, bool>? filter = null)
    {
        foreach (PacketFlags flag in Enum.GetValues<PacketFlags>())
        {
            if ((packet.Flags & flag) == flag && (filter == null || filter(flag)))
                yield return flag;
        }
    }

    /// <summary>
    /// Kiểm tra xem một cờ cụ thể có được bật trong gói tin hay không.
    /// </summary>
    public static bool HasFlag(PacketFlags flags, PacketFlags flagToCheck) =>
        (flags & flagToCheck) == flagToCheck;

    /// <summary>
    /// Hiển thị danh sách các cờ hiện đang bật.
    /// </summary>
    public static string GetActiveFlags(PacketFlags flags)
    {
        if (flags == PacketFlags.NONE)
            return "None";

        var activeFlags = Enum.GetValues<PacketFlags>();
        var result = "";

        foreach (PacketFlags flag in activeFlags)
        {
            if (HasFlag(flags, flag) && flag != PacketFlags.NONE)
            {
                result += $"{flag}, ";
            }
        }

        return result.TrimEnd(',', ' ');
    }
}