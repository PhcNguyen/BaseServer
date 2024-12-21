namespace NPServer.Core.Packets.Metadata;

/// <summary>
/// Represents different types of packets used in the network.
/// </summary>
public enum PacketType : byte
{
    /// <summary>
    /// No packet type set.
    /// </summary>
    NONE = 0,

    /// <summary>
    /// Text packet type.
    /// </summary>
    TEXT = 1,

    /// <summary>
    /// Image packet type.
    /// </summary>
    IMAGE = 2,

    /// <summary>
    /// Audio packet type.
    /// </summary>
    AUDIO = 4,

    /// <summary>
    /// Video packet type.
    /// </summary>
    VIDEO = 8,

    /// <summary>
    /// Persistent packet type for long-term storage.
    /// </summary>
    PERSISTENT = 16,

    /// <summary>
    /// Temporary packet type.
    /// </summary>
    TEMPORARY = 32,

    /// <summary>
    /// Partial packet type.
    /// </summary>
    PARTIAL = 64,

    /// <summary>
    /// File packet type.
    /// </summary>
    FILE = 128,
}
