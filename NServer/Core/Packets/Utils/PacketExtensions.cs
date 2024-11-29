using System;

using NServer.Core.Packets.Metadata;
using NServer.Core.Interfaces.Packets;

namespace NServer.Core.Packets.Utils
{
    /// <summary>
    /// Cung cấp các tiện ích mở rộng cho việc xử lý gói tin.
    /// </summary>
    internal static class PacketExtensions
    {
        public static readonly Packet EmptyPacket = new(0, 0, 0, []);

        public static Packet BuildResponse(short command, string message)
        {
            var packet = new Packet();

            packet.SetCmd(command);
            packet.SetPayload(message);
            return packet;
        }

        /// <summary>
        /// Tạo một gói tin từ mảng byte.
        /// </summary>
        /// <param name="data">Mảng byte chứa dữ liệu gói tin.</param>
        /// <returns>Đối tượng <see cref="Packet"/> được tạo từ dữ liệu.</returns>
        /// <exception cref="ArgumentException">Nếu dữ liệu không hợp lệ.</exception>
        public static IPacket FromByteArray(byte[] data)
        {
            if (data == null || data.Length < PacketMetadata.HEADERSIZE)
            {
                throw new ArgumentException("Invalid data length.", nameof(data));
            }

            var span = data.AsSpan();

            // Đọc Length và kiểm tra
            int length = BitConverter.ToInt32(span[0..sizeof(int)]);
            if (length > data.Length || length < PacketMetadata.HEADERSIZE)
            {
                throw new ArgumentException("Invalid packet length.", nameof(data));
            }

            byte type = span[PacketMetadata.TYPEOFFSET];
            byte flags = span[PacketMetadata.FLAGSOFFSET];
            short command = BitConverter.ToInt16(span[PacketMetadata.COMMANDOFFSET..]);
            byte[] payload = span[(PacketMetadata.PAYLOADOFFSET)..length].ToArray();

            // Tạo Packet từ dữ liệu
            return new Packet(type, flags, command, payload);
        }

        /// <summary>
        /// Lấy chiều dài của gói tin từ header.
        /// </summary>
        public static int GetPacketLength(byte[] packet)
        {
            return BitConverter.ToInt32(packet, PacketMetadata.LENGHTOFFSET);
        }

        /// <summary>
        /// Lấy cờ (flags) từ gói tin.
        /// </summary>
        public static byte GetPacketFlags(byte[] packet)
        {
            return packet[PacketMetadata.FLAGSOFFSET];
        }

        /// <summary>
        /// Lấy lệnh (command) từ gói tin.
        /// </summary>
        public static short GetPacketCommand(byte[] packet)
        {
            return BitConverter.ToInt16(packet, PacketMetadata.COMMANDOFFSET);
        }
    }
}