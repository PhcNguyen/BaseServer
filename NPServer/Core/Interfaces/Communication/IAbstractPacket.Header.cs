using NPServer.Core.Communication.Metadata;

namespace NPServer.Core.Interfaces.Communication
{
    public partial interface IAbstractPacket
    {
        PacketType Type { get; }
        PacketFlags Flags { get; }
        short Cmd { get; }

        void SetType(PacketType type);

        void EnableFlag(PacketFlags flag);

        void DisableFlag(PacketFlags flag);

        bool HasFlag(PacketFlags flag);

        void SetCmd(short command);

        void SetCmd(object command);
    }
}