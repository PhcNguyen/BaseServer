namespace NPServer.Core.Communication.Metadata
{
    [System.Flags]
    public enum PacketFlags : byte
    {
        NONE = 0,         // Không có cờ nào
        ISCOMPRESSED = 1, // Cờ đánh dấu gói tin đã được nén
        ISENCRYPTED = 2,  // Cờ đánh dấu gói tin đã được mã hóa
        ISRELIABLE = 4,   // Cờ đánh dấu gói tin là đáng tin cậy

        LOW = 8,          // Ưu tiên thấp
        MEDIUM = 16,      // Ưu tiên trung bình
        HIGH = 32,        // Ưu tiên cao
        URGENT = 64       // Cờ đánh dấu gói tin là khẩn cấp
    }
}