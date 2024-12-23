namespace NPServer.Common.Packets.Metadata;

/// <summary>
/// Packet flags indicating different states of a packet.
/// </summary>
[System.Flags]
public enum PacketFlags : byte
{
    /// <summary>
    /// No flags set.
    /// </summary>
    None = 0,

    /// <summary>
    /// Flag indicating that the packet is compressed.
    /// </summary>
    IsCompressed = 1,

    /// <summary>
    /// Flag indicating that the packet is encrypted.
    /// </summary>
    IsEncrypted = 2,

    /// <summary>
    /// Flag indicating that the packet is reliable.
    /// </summary>
    IsReliable = 4,

    /// <summary>
    /// Low priority.
    /// </summary>
    Low = 8,

    /// <summary>
    /// Medium priority.
    /// </summary>
    Medium = 16,

    /// <summary>
    /// High priority.
    /// </summary>
    High = 32,

    /// <summary>
    /// Flag indicating that the packet is urgent.
    /// </summary>
    Urgent = 64
}
