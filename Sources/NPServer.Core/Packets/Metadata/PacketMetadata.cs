namespace NPServer.Core.Packets.Metadata;

/// <summary>
/// Metadata offsets and sizes for network packets.
/// </summary>
public static class PacketMetadata
{
    // Header Field Sizes
    /// <summary>
    /// Size of the length field in bytes.
    /// </summary>
    public const int LENGTHSIZE = sizeof(int);

    /// <summary>
    /// Size of the type field in bytes.
    /// </summary>
    public const int TYPESIZE = sizeof(byte);

    /// <summary>
    /// Size of the flags field in bytes.
    /// </summary>
    public const int FLAGSSIZE = sizeof(byte);

    /// <summary>
    /// Size of the command field in bytes.
    /// </summary>
    public const int COMMANDSIZE = sizeof(short);

    /// <summary>
    /// Total size of the header in bytes (includes length, type, flags, and command).
    /// </summary>
    public const int HEADERSIZE = LENGTHSIZE + TYPESIZE + FLAGSSIZE + COMMANDSIZE;

    // Header Offsets
    /// <summary>
    /// Offset for the length field.
    /// </summary>
    public const int LENGTHOFFSET = 0;

    /// <summary>
    /// Offset for the type field.
    /// </summary>
    public const int TYPEOFFSET = LENGTHOFFSET + LENGTHSIZE;

    /// <summary>
    /// Offset for the flags field.
    /// </summary>
    public const int FLAGSOFFSET = TYPEOFFSET + TYPESIZE;

    /// <summary>
    /// Offset for the command field.
    /// </summary>
    public const int COMMANDOFFSET = FLAGSOFFSET + FLAGSSIZE;

    /// <summary>
    /// The offset for the payload data, starts right after the header.
    /// </summary>
    public const int PAYLOADOFFSET = COMMANDOFFSET + COMMANDSIZE;
}
