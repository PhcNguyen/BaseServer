namespace ToolClient.Core.Packets.Metadata
{
    internal struct PacketMetadata
    {
        public const int LENGHTSIZE = sizeof(int);
        public const int TYPESIZE = sizeof(byte);
        public const int FLAGSSIZE = sizeof(byte);
        public const int COMMANDSIZE = sizeof(sbyte);
        public const int HEADERSIZE = LENGHTSIZE + TYPESIZE + FLAGSSIZE + COMMANDSIZE;
        public const int LENGHTOFFSET = 0;
        public const int TYPEOFFSET = LENGHTOFFSET + LENGHTSIZE;
        public const int FLAGSOFFSET = TYPEOFFSET + TYPESIZE;
        public const int COMMANDOFFSET = FLAGSOFFSET + FLAGSSIZE;
        public const int PAYLOADOFFSET = COMMANDOFFSET + COMMANDSIZE;
    }
}