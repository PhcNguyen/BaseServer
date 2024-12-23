using NPServer.Common.Packets.Metadata;
using System;

namespace NPServer.Core.Packets.Utilities;

/// <summary>
/// Tiện ích kiểm tra và xác thực gói tin.
/// </summary>
public static class PacketValidation
{
    /// <summary>
    /// Kiểm tra cấu trúc của gói tin có hợp lệ hay không.
    /// </summary>
    /// <param name="packet">Mảng byte chứa dữ liệu gói tin.</param>
    /// <returns>Trả về `true` nếu gói tin hợp lệ, ngược lại là `false`.</returns>
    public static bool ValidatePacketStructure(byte[] packet)
    {
        if (!HasValidSize(packet)) return false;
        if (!HasValidLength(packet)) return false;

        return true;
    }

    /// <summary>
    /// Kiểm tra kích thước tối thiểu của gói tin.
    /// </summary>
    private static bool HasValidSize(byte[] packet) =>
        packet != null && packet.Length >= PacketMetadata.HEADERSIZE;

    /// <summary>
    /// Kiểm tra chiều dài của gói tin dựa trên header.
    /// </summary>
    private static bool HasValidLength(byte[] packet)
    {
        int length = BitConverter.ToInt32(packet, PacketMetadata.LENGTHOFFSET);
        return length > 0 && length <= packet.Length - PacketMetadata.HEADERSIZE;
    }
}