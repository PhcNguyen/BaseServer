using System;
using Base.Core.Packets.Metadata;

namespace Base.Core.Packets.Utils
{
    /// <summary>
    /// Tiện ích kiểm tra và xác thực gói tin.
    /// </summary>
    internal static class PacketValidationUtils
    {
        /// <summary>
        /// Kiểm tra xem gói tin có hợp lệ hay không.
        /// </summary>
        public static bool IsValidPacket(byte[] packet)
        {
            if (packet.Length < PacketMetadata.HEADERSIZE) return false;

            int length = BitConverter.ToInt32(packet, PacketMetadata.LENGHTOFFSET);
            return length > 0 && length <= packet.Length - PacketMetadata.HEADERSIZE;
        }
    }
}
