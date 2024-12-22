namespace NPServer.Core.Packets.Metadata;

/// <summary>
/// Represents different types of packets used in the network.
/// </summary>
public enum PacketType : byte
{
    /// <summary>
    /// No packet type set.
    /// </summary>
    None = 0,

    /// <summary>
    /// Int packet type.
    /// </summary>
    Int = 1,

    /// <summary>
    /// String packet type.
    /// </summary>
    String = 2,

    /// <summary>
    /// List packet type.
    /// </summary>
    List = 4,

    /// <summary>
    /// Long packet type.
    /// </summary>
    Long = 8,

    /// <summary>
    /// Xaml packet type.
    /// </summary>
    Xaml = 16,

    /// <summary>
    /// Temporary packet type.
    /// </summary>
    Json = 32,

    /// <summary>
    /// File packet type.
    /// </summary>
    File = 64,
}