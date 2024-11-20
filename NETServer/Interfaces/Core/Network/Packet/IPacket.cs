using NETServer.Application.Handlers;
using NETServer.Core.Network.Packet.Enums;

namespace NETServer.Interfaces.Core.Network.Packet
{
    /// <summary>
    /// Interface cho gói tin.
    /// </summary>
    internal interface IPacket
    {
        Guid Id { get; }
        byte Version { get; }
        PacketFlags Flags { get; }
        short Command { get; }
        Memory<byte> Payload { get; }

        void Reset();
        void SetVersion(byte version);

        void SetFlag(PacketFlags flag);
        void RemoveFlag(PacketFlags flag);
        bool HasFlag(PacketFlags flag);

        void SetCommand(short command);
        void SetCommand(Cmd command);

        void SetPayload(ReadOnlySpan<byte> newPayload);
        void SetPayload(string newPayload);

        bool IsValid();
    }
}
