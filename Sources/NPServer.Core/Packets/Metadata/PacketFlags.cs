namespace NPServer.Core.Packets.Metadata;

/// <summary>
/// Packet flags indicating different states of a packet.
/// </summary>
[System.Flags]
public enum PacketFlags : byte
{
    /// <summary>
    /// No flags set.
    /// </summary>
    NONE = 0,

    /// <summary>
    /// Flag indicating that the packet is compressed.
    /// </summary>
    ISCOMPRESSED = 1,

    /// <summary>
    /// Flag indicating that the packet is encrypted.
    /// </summary>
    ISENCRYPTED = 2,

    /// <summary>
    /// Flag indicating that the packet is reliable.
    /// </summary>
    ISRELIABLE = 4,

    /// <summary>
    /// Low priority.
    /// </summary>
    LOW = 8,

    /// <summary>
    /// Medium priority.
    /// </summary>
    MEDIUM = 16,

    /// <summary>
    /// High priority.
    /// </summary>
    HIGH = 32,

    /// <summary>
    /// Flag indicating that the packet is urgent.
    /// </summary>
    URGENT = 64
}
