namespace NServer.Core.Packets.Enums;

/// <summary>
/// Enum định nghĩa các loại gói tin trong hệ thống.
/// </summary>
public enum PacketType : byte
{
    NONE,

    /// <summary>
    /// Gói tin chứa thông điệp chat.
    /// </summary>
    CHATMESSAGE,

    /// <summary>
    /// Gói tin chứa thông điệp thì thầm.
    /// </summary>
    WHISPERMESSAGE,

    /// <summary>
    /// Gói tin chứa thông tin di chuyển của người chơi.
    /// </summary>
    PLAYERMOVEMENT,

    /// <summary>
    /// Gói tin chứa thông tin hành động tấn công.
    /// </summary>
    ATTACKACTION,

    /// <summary>
    /// Gói tin chứa thông tin sử dụng kỹ năng.
    /// </summary>
    SKILLUSAGE,

    /// <summary>
    /// Gói tin chứa thông tin trạng thái người chơi.
    /// </summary>
    PLAYERSTATUS,

    /// <summary>
    /// Gói tin chứa thông tin trạng thái nhóm.
    /// </summary>
    PARTYSTATUS,

    /// <summary>
    /// Gói tin chứa thông tin cập nhật nhiệm vụ.
    /// </summary>
    QUESTUPDATE,

    /// <summary>
    /// Gói tin chứa thông tin nhặt đồ.
    /// </summary>
    ITEMPICKUP,
}