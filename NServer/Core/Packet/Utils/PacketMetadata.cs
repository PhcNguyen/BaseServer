namespace NServer.Core.Packet.Utils
{
    /// <summary>
    /// Metadata offsets and size for network packets.
    /// </summary>
    internal struct PacketMetadata
    {
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
        /// Total size of the header in bytes.
        /// </summary>
        public const int HEADERSIZE = sizeof(byte) + sizeof(short) + sizeof(int);

        public const int PAYLOADOFFSET = HEADERSIZE;
    }

}
