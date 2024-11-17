using System.Runtime.CompilerServices;

namespace NETServer.Network.Packets
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
        public static Packet CreatePacket(byte version, byte flags, short command, byte[] payload)
        {
            return new Packet(
                id: null,
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
        public static Packet? CreatePacket(byte[] data)
        {
            if (data.Length < PacketMetadata.HEADERSIZE)  
                return null;

            return new Packet(
                id: null,
                version: data[PacketMetadata.VERSIONOFFSET],
                flags: data[PacketMetadata.FLAGSOFFSET],
                command: BitConverter.ToInt16(data, PacketMetadata.COMMANDOFFSET),
                payload: data[PacketMetadata.HEADERSIZE..]  
            );
        }
    }
}
