using NServer.Core.Packets.Enums;

namespace NServer.Core.Interfaces.Packets
{
    public partial interface IPacket
    {
        PacketType Type { get; }
        Packet Flags { get; }
        short Cmd { get; }

        void SetType(PacketType type);

        void AddFlag(Packet flag);

        bool HasFlag(Packet flag);

        void RemoveFlag(Packet flag);

        void SetCmd(short command);

        void SetCmd(object command);
    }
}