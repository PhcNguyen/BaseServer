namespace ToolClient.Core.Packets.Enums
{
    /// <summary>
    /// Enum định nghĩa các loại gói tin trong hệ thống.
    /// </summary>
    internal enum PacketType : byte
    {
        NONE,
        CHATMESSAGE,
        WHISPERMESSAGE,
        PLAYERMOVEMENT,
        ATTACKACTION,
        SKILLUSAGE,
        PLAYERSTATUS,
        PARTYSTATUS,
        QUESTUPDATE,
        ITEMPICKUP,
    }
}