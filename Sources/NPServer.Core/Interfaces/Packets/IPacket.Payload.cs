using System;
using System.Collections.Generic;

namespace NPServer.Core.Interfaces.Packets;

public partial interface IPacket
{
    Memory<byte> PayloadData { get; }

    void SetPayload(string newPayload);

    void SetPayload(Span<byte> newPayload);

    void AddToPayload(ReadOnlyMemory<byte> additionalData);

    bool RemoveFromPayload(int startIndex, int length);

    bool ReplaceInPayload(int startIndex, ReadOnlyMemory<byte> newData);

    void AddMultiplePayloads(IEnumerable<ReadOnlyMemory<byte>> payloads);
}