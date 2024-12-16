using NPServer.Core.Interfaces.Memory;
using NPServer.Infrastructure.Services;
using System;

namespace NPServer.Core.Interfaces.Packets;

public partial interface IPacket : IPoolable
{
    UniqueId Id { get; }

    new void ResetForPool();

    string ToJson();

    byte[] ToByteArray();

    bool ParseFromBytes(ReadOnlySpan<byte> data);

    void SetId(UniqueId id);
}