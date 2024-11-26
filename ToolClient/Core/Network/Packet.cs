

using System.Buffers;
using System.Security.Cryptography;

namespace ToolClient.Core.Network
{
    [Flags]
    internal enum PacketFlags : byte
    {
        NONE = 0,         // Không có cờ nào
        ISCOMPRESSED = 1, // Cờ đánh dấu gói tin đã được nén
        ISENCRYPTED = 2,  // Cờ đánh dấu gói tin đã được mã hóa
        ISRELIABLE = 4,   // Cờ đánh dấu gói tin là đáng tin cậy

        LOW = 8,          // Ưu tiên thấp
        MEDIUM = 16,      // Ưu tiên trung bình
        HIGH = 32,        // Ưu tiên cao
        ISURGENT = 64     // Cờ đánh dấu gói tin là khẩn cấp
    }

    internal class Packet(byte flags, short command, byte type, byte[] payload)
    {
        private const int HeaderSize = 12;
        public PacketFlags Flags { get; set; } = (PacketFlags)flags;
        public short Command { get; set; } = command;
        public byte Type { get; set; } = type;
        public byte[] Payload { get; set; } = payload ?? [];

        public int Length => HeaderSize + (Payload?.Length ?? 0);


        public byte[] ToByteArray()
        {
            // Thuê bộ nhớ từ ArrayPool
            byte[] packet = ArrayPool<byte>.Shared.Rent(Length);

            try
            {
                var span = packet.AsSpan(0, Length);

                // Header
                BitConverter.TryWriteBytes(span[..4], Length); // Ghi độ dài
                span[4] = (byte)Flags;                         // Ghi cờ (Flags)
                BitConverter.TryWriteBytes(span[5..], Command); // Ghi lệnh (Command)
                span[7] = (byte)Type;                          // Ghi kiểu (Type)

                // Payload
                Payload.CopyTo(span[8..(Length - 4)]);         // Sao chép payload

                // Checksum
                int checksum = CalculateChecksum(span[..(Length - 4)]);
                BitConverter.TryWriteBytes(span[(Length - 4)..], checksum);

                // Trả về mảng đã được copy (đảm bảo không rò rỉ bộ nhớ)
                return span[..Length].ToArray();
            }
            finally
            {
                // Trả lại bộ nhớ cho ArrayPool
                ArrayPool<byte>.Shared.Return(packet);
            }
        }

        public static Packet FromByteArray(byte[] data)
        {
            if (data == null || data.Length < HeaderSize)
            {
                throw new ArgumentException($"Data is null or smaller than header size ({HeaderSize} bytes).", nameof(data));
            }

            var span = data.AsSpan();

            // Đọc Length và kiểm tra
            int length = BitConverter.ToInt32(span[..4]);
            if (length > data.Length || length < HeaderSize)
            {
                throw new ArgumentException($"Invalid packet length: {length}.", nameof(data));
            }

            // Xác minh checksum
            int expectedChecksum = BitConverter.ToInt32(span[(length - 4)..]);
            int actualChecksum = CalculateChecksum(span[..(length - 4)]);
            if (expectedChecksum != actualChecksum)
            {
                throw new ArgumentException("Checksum mismatch. Data might be corrupted.", nameof(data));
            }

            // Đọc các trường header
            byte flags = span[4];
            short command = BitConverter.ToInt16(span[5..7]);
            byte type = span[7];

            // Đọc payload
            byte[] payload = span[8..(length - 4)].ToArray();

            // Trả về đối tượng Packet
            return new Packet(flags, command, type, payload);
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
            if (data == null || data.Length < 8)
            {
                throw new ArgumentException("Invalid data length.", nameof(data));
            }

            var span = data.AsSpan();

            // Đọc Length và kiểm tra
            int length = BitConverter.ToInt32(span[0..sizeof(int)]);
            if (length > data.Length || length < 8)
            {
                throw new ArgumentException("Invalid packet length.", nameof(data));
            }

            // Kiểm tra checksum
            ReadOnlySpan<byte> dataWithoutChecksum = span[..(length - 4)];
            int expectedChecksum = BitConverter.ToInt32(span[(length - 4)..]);

            int actualChecksum = CalculateChecksum(dataWithoutChecksum);

            return expectedChecksum == actualChecksum;  // Trả về true nếu checksum hợp lệ
        }
    }
}
