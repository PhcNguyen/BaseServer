using System;
using System.Collections.Generic;
using System.Security.Cryptography;

using NServer.Core.Packets.Enums;

namespace NServer.Core.Packets.Utils
{
    /// <summary>
    /// Cung cấp các tiện ích mở rộng cho việc xử lý gói tin.
    /// </summary>
    internal static class PacketExtensions
    {
        /// <summary>
        /// Tạo một gói tin từ mảng byte.
        /// </summary>
        /// <param name="data">Mảng byte chứa dữ liệu gói tin.</param>
        /// <returns>Đối tượng <see cref="Packet"/> được tạo từ dữ liệu.</returns>
        /// <exception cref="ArgumentException">Nếu dữ liệu không hợp lệ.</exception>
        public static Packet FromByteArray(byte[] data)
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

            byte flags = span[PacketMetadata.FLAGSOFFSET];
            short command = BitConverter.ToInt16(span[PacketMetadata.COMMANDOFFSET..]);
            byte[] payload = span[(PacketMetadata.PAYLOADOFFSET)..length].ToArray();

            // Tạo Packet từ dữ liệu
            return new Packet(flags, command, payload);
        }

        /// <summary>
        /// Tính toán checksum của dữ liệu.
        /// </summary>
        public static int CalculateChecksum(ReadOnlySpan<byte> data)
        {
            // Sử dụng SHA256 trên Span để tránh sao chép bộ nhớ không cần thiết
            byte[] hashBytes = SHA256.HashData(data);
            return BitConverter.ToInt32(hashBytes, 0);  // Chuyển đổi 4 byte đầu tiên thành checksum
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của checksum.
        /// </summary>
        public static bool VerifyChecksum(byte[] data)
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

            // Kiểm tra checksum
            ReadOnlySpan<byte> dataWithoutChecksum = span[..(length)];
            int expectedChecksum = BitConverter.ToInt32(span[(length - PacketMetadata.CHECKSUMSIZE)..]);

            int actualChecksum = CalculateChecksum(dataWithoutChecksum);

            return expectedChecksum == actualChecksum;  // Trả về true nếu checksum hợp lệ
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

        /// <summary>
        /// Kiểm tra xem gói tin có hợp lệ hay không.
        /// </summary>
        public static bool IsValidPacket(byte[] packet)
        {
            if (packet.Length < PacketMetadata.HEADERSIZE) return false;

            int length = BitConverter.ToInt32(packet, PacketMetadata.LENGHTOFFSET);
            return length > 0 && length <= packet.Length - PacketMetadata.HEADERSIZE;
        }

        /// <summary>
        /// Xác định độ ưu tiên của gói tin dựa trên cờ (flags).
        /// </summary>
        public static int DeterminePriority(Packet packet) => packet.Flags switch
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
        public static IEnumerable<PacketFlags> CombinedFlags(this Packet packet, Func<PacketFlags, bool>? filter = null)
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