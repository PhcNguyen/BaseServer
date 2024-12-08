namespace NPServer.Core.Communication.Metadata
{
    /// <summary>
    /// Metadata offsets and sizes for network packets.
    /// </summary>
    public static class PacketMetadata
    {
        // Header Field Sizes
        public const int LENGTHSIZE = sizeof(int);     // Size of the length (4 bytes)

        public const int TYPESIZE = sizeof(byte);      // Size of the type (1 byte)
        public const int FLAGSSIZE = sizeof(byte);     // Size of the flags (1 byte)
        public const int COMMANDSIZE = sizeof(short);  // Size of the command (2 bytes)

        /// <summary>
        /// Total size of the header in bytes (includes length, type, flags, and command).
        /// </summary>
        public const int HEADERSIZE = LENGTHSIZE + TYPESIZE + FLAGSSIZE + COMMANDSIZE;

        // Header Offsets
        public const int LENGTHOFFSET = 0;                         // Offset for the length field

        public const int TYPEOFFSET = LENGTHOFFSET + LENGTHSIZE;   // Offset for the type field
        public const int FLAGSOFFSET = TYPEOFFSET + TYPESIZE;     // Offset for the flags field
        public const int COMMANDOFFSET = FLAGSOFFSET + FLAGSSIZE; // Offset for the command field

        /// <summary>
        /// The offset for the payload data, starts right after the header.
        /// </summary>
        public const int PAYLOADOFFSET = COMMANDOFFSET + COMMANDSIZE;
    }
}