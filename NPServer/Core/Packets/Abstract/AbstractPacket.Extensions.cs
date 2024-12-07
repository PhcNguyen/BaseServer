﻿using NPServer.Core.Packets.Metadata;
using System;
using System.Buffers;
using System.Text;

namespace NPServer.Core.Packets.Base;

public partial class AbstractPacket
{
    /// <summary>
    /// Chuyển đổi gói tin thành mảng byte để gửi qua mạng.
    /// </summary>
    /// <returns>Mảng byte của gói tin.</returns>
    public virtual byte[] ToByteArray()
    {
        // Sử dụng ArrayPool để giảm chi phí bộ nhớ heap
        byte[] packet = ArrayPool<byte>.Shared.Rent(Length);

        try
        {
            Span<byte> span = packet.AsSpan(0, Length);

            // Header
            BitConverter.TryWriteBytes(span[..], Length); // Ghi chiều dài gói tin
            span[PacketMetadata.TYPEOFFSET] = (byte)Type;
            span[PacketMetadata.FLAGSOFFSET] = (byte)Flags;
            BitConverter.TryWriteBytes(span[PacketMetadata.COMMANDOFFSET..], Cmd);

            // Payload
            _payload.Span.CopyTo(span[PacketMetadata.PAYLOADOFFSET..]);

            return span[..Length].ToArray();
        }
        finally
        {
            // Đảm bảo trả lại bộ nhớ vào ArrayPool ngay cả khi có ngoại lệ
            ArrayPool<byte>.Shared.Return(packet);
        }
    }

    /// <summary>
    /// Parse dữ liệu từ mảng byte để tạo một gói tin.
    /// </summary>
    /// <param name="data">Mảng byte chứa dữ liệu gói tin.</param>
    /// <exception cref="ArgumentException">Khi dữ liệu không hợp lệ hoặc không đủ dài.</exception>
    public void ParseFromBytes(ReadOnlySpan<byte> data)
    {
        // Kiểm tra dữ liệu có đủ nhỏ nhất để chứa header
        if (data.Length < PacketMetadata.HEADERSIZE)
            throw new ArgumentException("Data length is too short to be a valid packet.");

        // Header
        int length = BitConverter.ToInt32(data[..PacketMetadata.LENGHTOFFSET]);
        if (data.Length < length)
            throw new ArgumentException("Data length does not match packet length.");

        Type = (PacketType)data[PacketMetadata.TYPEOFFSET];
        Flags = (PacketFlags)data[PacketMetadata.FLAGSOFFSET];
        Cmd = BitConverter.ToInt16(data[PacketMetadata.COMMANDOFFSET..]);

        // Payload
        ReadOnlySpan<byte> Payload = data[PacketMetadata.PAYLOADOFFSET..length];
    }

    /// <summary>
    /// Chuyển đổi gói tin thành chuỗi JSON.
    /// </summary>
    /// <returns>Chuỗi JSON đại diện cho gói tin.</returns>
    public string ToJson()
    {
        var json = new
        {
            Flags,
            Cmd,
            PayloadLength = _payload.Length,
            Payload = Encoding.UTF8.GetString(Payload.ToArray())
        };

        return System.Text.Json.JsonSerializer.Serialize(json);
    }
}