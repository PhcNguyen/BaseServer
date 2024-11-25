namespace NServer.Core.Packets.Utils
{
    /// <summary>
    /// Metadata offsets and size for network packets.
    /// </summary>
    internal struct PacketMetadata
    {
        /// <summary>
        /// Size of the length (4 bytes).
        /// </summary>
        public const int LENGHTSIZE = sizeof(int);

        /// <summary>
        /// Size of the flags (1 byte).
        /// </summary>
        public const int FLAGSSIZE = sizeof(byte);

        /// <summary>
        /// Size of the command (2 bytes).
        /// </summary>
        public const int COMMANDSIZE = sizeof(sbyte);

        /// <summary>
        /// Size of the type (1 bytes).
        /// </summary>
        public const int TYPESIZE = sizeof(byte);

        /// <summary>
        /// Size of the checksum (4 bytes).
        /// </summary>
        public const int CHECKSUMSIZE = sizeof(int);

        /// <summary>
        /// Total size of the header in bytes.
        /// </summary>
        public const int HEADERSIZE = LENGHTSIZE + FLAGSSIZE + COMMANDSIZE + TYPESIZE;



        /// <summary>
        /// Offset for the length field (4 bytes).
        /// </summary>
        public const int LENGHTOFFSET = 0;

        /// <summary>
        /// Offset for the flags field (1 byte).
        /// </summary>
        public const int FLAGSOFFSET = 4;

        /// <summary>
        /// Offset for the command field (2 bytes).
        /// </summary>
        public const int COMMANDOFFSET = 5;

        /// <summary>
        /// Offset for the type field (1 bytes).
        /// </summary>
        public const int TYPEOFFSET = 3;

        /// <summary>
        /// The offset for the payload data, starts after the header.
        /// </summary>
        public const int PAYLOADOFFSET = HEADERSIZE + 1;
    }
}
