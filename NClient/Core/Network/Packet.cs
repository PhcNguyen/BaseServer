namespace NClient.Core.Network
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

    internal class Packet(byte flags, short command, byte[] payload)
    {
        private const int HeaderSize = 7;
        public PacketFlags Flags { get; set; } = (PacketFlags)flags;
        public short Command { get; set; } = command;
        public byte[] Payload { get; set; } = payload;

        public int Length => HeaderSize + (Payload?.Length ?? 0);

        public byte[] ToByteArray()
        {
            using var stream = new MemoryStream();

            // Header
            stream.Write(BitConverter.GetBytes(Length), 0, 4);
            stream.WriteByte((byte)Flags);
            stream.Write(BitConverter.GetBytes(Command), 0, 2);
            
            // Payload
            if (Payload != null && Payload.Length > 0)
                stream.Write(Payload, 0, Payload.Length);

            return stream.ToArray();
        }

        public static Packet FromByteArray(byte[] data)
        {
            if (data == null || data.Length < HeaderSize)
            {
                throw new ArgumentException("Invalid data length.", nameof(data));
            }

            var span = data.AsSpan();

            // Đọc Length và kiểm tra
            int length = BitConverter.ToInt32(span[0..sizeof(int)]);
            if (length > data.Length || length < HeaderSize)
            {
                throw new ArgumentException("Invalid packet length.", nameof(data));
            }

            // Lấy Flags và Command từ mảng byte
            byte flags = span[4];
            short command = BitConverter.ToInt16(span[5..7]);

            // Payload
            byte[] payload = span[7..length].ToArray();

            return new Packet(flags, command, payload);
        }
    }
}
