using NPServer.Core.Packets.Metadata;

namespace NPServer.Core.Interfaces.Packets;

public partial interface IPacket
{
    PacketType Type { get; }
    PacketFlags Flags { get; }
    short Cmd { get; }

    void SetType(PacketType type);

    void EnableFlag(PacketFlags flag);

    void DisableFlag(PacketFlags flag);

    bool HasFlag(PacketFlags flag);

    void SetCmd(short command);

    void SetCmd(System.Enum command);
}