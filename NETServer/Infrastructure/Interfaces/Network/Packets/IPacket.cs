using NETServer.Network.Packets;

namespace NETServer.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface cho gói tin.
    /// </summary>
    internal interface IPacket
    {
        byte Version { get; }
        PacketFlags Flags { get; }
        short? Command { get; }
        Memory<byte> Payload { get; }

        void Reset();
        void SetVersion(byte version);
        void SetFlag(PacketFlags flag);
        void RemoveFlag(PacketFlags flag);
        bool HasFlag(PacketFlags flag);
        void SetCommand(short command);
        void SetPayload(byte[] newPayload);
        bool IsValid();
    }
}
