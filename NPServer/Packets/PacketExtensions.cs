using NPServer.Core.Communication.Metadata;
using System;

namespace NPServer.Packets
{
    /// <summary>
    /// Cung cấp các tiện ích mở rộng cho việc xử lý gói tin.
    /// </summary>
    public static class PacketExtensions
    {
        public static readonly Packet EmptyPacket = new(0, 0, 0, []);

        public static Packet ToResponsePacket(this short command, string message)
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
        public static Packet ParseFromBytes(this byte[] data)
        {
            ValidatePacketData(data); // Kiểm tra dữ liệu
            return DeserializePacket(data.AsSpan()); // Phân tích và tạo packet
        }

        /// <summary>
        /// Lấy kiểu (type) từ gói tin.
        /// </summary>
        public static byte ExtractType(this byte[] packet) =>
            packet[PacketMetadata.TYPEOFFSET];

        /// <summary>
        /// Lấy cờ (flags) từ gói tin.
        /// </summary>
        public static byte ExtractFlags(this byte[] packet) =>
            packet[PacketMetadata.FLAGSOFFSET];

        /// <summary>
        /// Lấy lệnh (command) từ gói tin.
        /// </summary>
        public static short ExtractCommand(this byte[] packet) =>
            BitConverter.ToInt16(packet, PacketMetadata.COMMANDOFFSET);

        /// <summary>
        /// Lấy chiều dài của gói tin từ header.
        /// </summary>
        public static int GetHeaderLength(this byte[] packet) =>
            BitConverter.ToInt32(packet, PacketMetadata.LENGTHOFFSET);

        private static void ValidatePacketData(byte[] data)
        {
            if (data == null || data.Length < PacketMetadata.HEADERSIZE)
            {
                throw new ArgumentException("Invalid data length.", nameof(data));
            }

            int length = BitConverter.ToInt32(data, PacketMetadata.LENGTHOFFSET);
            if (length > data.Length || length < PacketMetadata.HEADERSIZE)
            {
                throw new ArgumentException("Invalid packet length.", nameof(data));
            }
        }

        private static Packet DeserializePacket(ReadOnlySpan<byte> span)
        {
            int length = BitConverter.ToInt32(span[0..sizeof(int)]);
            byte type = span[PacketMetadata.TYPEOFFSET];
            byte flags = span[PacketMetadata.FLAGSOFFSET];
            short command = BitConverter.ToInt16(span[PacketMetadata.COMMANDOFFSET..]);
            byte[] payload = span[PacketMetadata.PAYLOADOFFSET..length].ToArray();

            return new Packet(type, flags, command, payload);
        }
    }
}