using NPServer.Core.Packets;

namespace NPServer.Core.Interfaces.Packets
{
    public partial interface IPacket
    {
        PacketType Type { get; }
        PacketFlags Flags { get; }
        short Cmd { get; }

        void SetType(PacketType type);

        void AddFlag(PacketFlags flag);

        bool HasFlag(PacketFlags flag);

        void RemoveFlag(PacketFlags flag);

        void SetCmd(short command);

        void SetCmd(object command);
    }
}