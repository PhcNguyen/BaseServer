namespace NPServer.Core.Communication.Metadata
{
    public enum PacketType : byte
    {
        NONE = 0,          // Không có loại gói tin nào
        TEXT = 1,          // Gói tin dạng văn bản
        IMAGE = 2,         // Gói tin dạng hình ảnh
        AUDIO = 4,         // Gói tin dạng âm thanh
        VIDEO = 8,         // Gói tin dạng video
        PERSISTENT = 16,   // Gói tin lưu trữ lâu dài
        TEMPORARY = 32,    // Gói tin tạm thời
        PARTIAL = 64,      // Gói tin một phần

        FILE = 128,        // Gói tin dạng file
    }
}
