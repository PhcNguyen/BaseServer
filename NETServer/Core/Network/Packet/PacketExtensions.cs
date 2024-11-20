using NETServer.Core.Network.Packet.Enums;
using System.Runtime.CompilerServices;

namespace NETServer.Core.Network.Packet
{
    /// <summary>
    /// Cung cấp các tiện ích mở rộng cho gói tin.
    /// </summary>
    internal static class PacketExtensions
    {
        /// <summary>
        /// Tạo Packet từ một chuỗi nội dung.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Packet CreatePacket(Guid id, byte? version = null,
            byte? flags = null, short? command = null, byte[]? payload = null)
        {
            return new Packet(
                id: id,
                version: version,
                flags: flags,
                command: command,
                payload: payload
            );
        }

        /// <summary>
        /// Tạo Packet từ một chuỗi nội dung.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Packet? CreatePacket(Guid id, byte[]? data = null)
        {
            if (data == null || data.Length < PacketMetadata.HEADERSIZE)
                return null;

            return new Packet(
                id: id,
                version: data[PacketMetadata.VERSIONOFFSET],
                flags: data[PacketMetadata.FLAGSOFFSET],
                command: BitConverter.ToInt16(data, PacketMetadata.COMMANDOFFSET),
                payload: data[PacketMetadata.HEADERSIZE..]
            );
        }


        public static int DeterminePriority(Packet packet) => packet.Flags switch
        {
            PacketFlags f when f.HasFlag(PacketFlags.ISURGENT) => 4, // Khẩn cấp
            PacketFlags f when f.HasFlag(PacketFlags.HIGH) => 3,     // Cao
            PacketFlags f when f.HasFlag(PacketFlags.MEDIUM) => 2,   // Trung bình
            PacketFlags f when f.HasFlag(PacketFlags.LOW) => 1,      // Thấp
            _ => 0,                                                  // Không có cờ ưu tiên nào
        };
    }
}
