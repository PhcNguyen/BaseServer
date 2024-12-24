using System;
using System.Buffers;
using System.Security.Cryptography;

namespace NPServer.Common.Packets;

public partial class Packet
{
    private byte[] _signature = [];

    private void SignPacket()
    {
        int bufferSize = this.CalculateBufferSize();

        Span<byte> buffer = bufferSize <= 1024
            ? stackalloc byte[bufferSize]
            : ArrayPool<byte>.Shared.Rent(bufferSize);

        try
        {
            this.FillSignableData(buffer);
            _signature = CreateSignature(buffer);
        }
        finally
        {
            if (buffer.Length > 1024)
                ArrayPool<byte>.Shared.Return(buffer.ToArray());
        }
    }

    private bool VerifySignature()
    {
        if (_signature == null || _signature.Length != 32)
            return false;

        int bufferSize = this.CalculateBufferSize();

        Span<byte> buffer = bufferSize <= 1024
            ? stackalloc byte[bufferSize]
            : ArrayPool<byte>.Shared.Rent(bufferSize);

        try
        {
            this.FillSignableData(buffer);

            Span<byte> expectedSignature = stackalloc byte[32];
            SHA256.HashData(buffer, expectedSignature);

            return _signature.AsSpan().SequenceEqual(expectedSignature);
        }
        finally
        {
            if (buffer.Length > 1024)
                ArrayPool<byte>.Shared.Return(buffer.ToArray());
        }
    }

    private int CalculateBufferSize()
    {
        return 2 + 1 + 1 + PayloadData.Length; // Cmd (2 bytes) + Type (1 byte) + Flags (1 byte) + Payload
    }

    private void FillSignableData(Span<byte> buffer)
    {
        BitConverter.TryWriteBytes(buffer[..2], Cmd); // Cmd: 2 bytes
        buffer[2] = (byte)Type;                      // Type: 1 byte
        buffer[3] = (byte)Flags;                     // Flags: 1 byte
        this.PayloadData.Span.CopyTo(buffer[4..]);   // PayloadData
    }

    private static byte[] CreateSignature(ReadOnlySpan<byte> signableData)
    {
        return SHA256.HashData(signableData);
    }
}