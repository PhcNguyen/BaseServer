using NServer.Core.Packets.Metadata;
using System;
using System.Text;
using System.Buffers;

namespace NServer.Core.Packets.Base;

public partial class BasePacket
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