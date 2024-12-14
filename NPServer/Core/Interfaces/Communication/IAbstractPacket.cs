using NPServer.Infrastructure.Services;
using System;

namespace NPServer.Core.Interfaces.Communication;

public partial interface IAbstractPacket
{
    UniqueId Id { get; }

    void ResetForPool();

    string ToJson();

    byte[] ToByteArray();

    void ParseFromBytes(ReadOnlySpan<byte> data);

    void SetId(UniqueId id);
}