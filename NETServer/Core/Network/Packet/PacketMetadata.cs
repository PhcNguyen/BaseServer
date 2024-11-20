namespace NETServer.Core.Network.Packet
{
    internal struct PacketMetadata
    {
        public const int LENGHTOFFSET = 0;   // 4 byte: int
        public const int VERSIONOFFSET = 4;  // 1 byte: byte
        public const int FLAGSOFFSET = 5;    // 1 byte: byte
        public const int COMMANDOFFSET = 6;  // 2 byte: short

        public const int HEADERSIZE = sizeof(byte) * 2 + sizeof(short) + sizeof(int); //8
    }
}
