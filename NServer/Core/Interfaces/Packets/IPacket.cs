using System;

using NServer.Core.Packets.Enums;
using NServer.Infrastructure.Services;

namespace NServer.Core.Interfaces.Packets
{
    internal interface IPacket
    {
        UniqueId Id { get; }
        short Command { get; }
        PacketFlags Flags { get; }
        byte Type { get; }
        Memory<byte> Payload { get; }

        byte[] ToByteArray();
        void Reset();
        void AppendToPayload(byte[] additionalData);
        string ToJson();

        void SetID(UniqueId id);
        void SetFlag(PacketFlags flag);
        void RemoveFlag(PacketFlags flag);
        bool HasFlag(PacketFlags flag);
        void SetCommand(short command);
        void SetCommand(object command);
        bool TrySetPayload(ReadOnlySpan<byte> newPayload);
        void SetPayload(string newPayload);
        void SetPayload(Span<byte> newPayload);
    }
}
