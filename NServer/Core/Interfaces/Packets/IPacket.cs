using NPServer.Core.Services;
using System;

namespace NPServer.Core.Interfaces.Packets
{
    public partial interface IPacket
    {
        UniqueId Id { get; }

        void Reset();

        string ToJson();

        byte[] ToByteArray();

        void ParseFromBytes(ReadOnlySpan<byte> data);

        void SetId(UniqueId id);
    }
}