using System;
using System.Collections.Generic;

namespace NServer.Core.Interfaces.Packets
{
    public partial interface IPacket
    {
        Memory<byte> Payload { get; }

        bool TrySetPayload(ReadOnlySpan<byte> newPayload);

        void SetPayload(string newPayload);

        void SetPayload(Span<byte> newPayload);

        void AppendToPayload(byte[] additionalData);

        bool RemovePayloadSection(int startIndex, int length);

        bool ReplacePayloadSection(int startIndex, byte[] newData);

        void AppendMultiplePayloads(IEnumerable<byte[]> payloads);
    }
}