using NServer.Core.Packets.Enums;

namespace NServer.Core.Interfaces.Packets
{
    internal partial interface IPacket
    {
        PacketFlags Flags { get; }
        PacketType Type { get; }
        short Command { get; }

        void SetType(PacketType type);
        void AddFlag(PacketFlags flag);
        void RemoveFlag(PacketFlags flag);
        bool HasFlag(PacketFlags flag);
        void SetCmd(short command);
        void SetCmd(object command);
    }
}
