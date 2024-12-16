using NPServer.Core.Packets.Metadata;
using NPServer.Core.Interfaces.Packets;
using System;
using System.Buffers;

namespace NPServer.Core.Packets;

public partial class Packet : IPacket
{
    /// <summary>
    /// Tổng chiều dài của gói tin, bao gồm header và payload.
    /// </summary>
    public int Length => _headerSize + _payload.Length;

    /// <summary>
    /// Chuyển đổi gói tin thành mảng byte để gửi qua mạng.
    /// </summary>
    /// <returns>Mảng byte của gói tin.</returns>
    public byte[] ToByteArray()
    {
        // Sử dụng ArrayPool để giảm chi phí bộ nhớ heap
        byte[] packet = ArrayPool<byte>.Shared.Rent(Length);

        try
        {
            this.SignPacket();

            Span<byte> span = packet.AsSpan(0, Length);

            // Header
            BitConverter.TryWriteBytes(span[..], Length); // Ghi chiều dài gói tin
            span[PacketMetadata.TYPEOFFSET] = (byte)Type;
            span[PacketMetadata.FLAGSOFFSET] = (byte)Flags;
            BitConverter.TryWriteBytes(span[PacketMetadata.COMMANDOFFSET..], Cmd);

            // Payload
            _payload.Span.CopyTo(span[PacketMetadata.PAYLOADOFFSET..]);

            // Copy signature
            Signature.CopyTo(span[Length..]);

            return span[..(Length + Signature.Length)].ToArray();
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
    /// <returns>True nếu phân tích thành công, ngược lại là False.</returns>
    public bool ParseFromBytes(ReadOnlySpan<byte> data)
    {
        // Kiểm tra dữ liệu có đủ nhỏ nhất để chứa header
        if (data.Length < PacketMetadata.HEADERSIZE)
            return false;

        // Header
        int length = BitConverter.ToInt32(data[..PacketMetadata.LENGTHOFFSET]);
        if (data.Length < length)
            return false;

        try
        {
            Type = (PacketType)data[PacketMetadata.TYPEOFFSET];
            Flags = (PacketFlags)data[PacketMetadata.FLAGSOFFSET];
            Cmd = BitConverter.ToInt16(data[PacketMetadata.COMMANDOFFSET..]);

            // Payload
            PayloadData = data[PacketMetadata.PAYLOADOFFSET..length].ToArray();

            // Signature
            Signature = data[length..].ToArray();

            if (!VerifySignature()) 
                return false;
        }
        catch
        {
            // Bắt mọi ngoại lệ và trả về false
            return false;
        }

        return true;
    }

    /// <summary>
    /// Chuyển đổi gói tin thành chuỗi JSON.
    /// </summary>
    /// <returns>Chuỗi JSON đại diện cho gói tin.</returns>
    public string ToJson()
    {
        string payloadString = _payload.Length > 0
            ? Convert.ToBase64String(PayloadData.ToArray())
            : string.Empty;

        var json = new
        {
            Flags,
            Cmd,
            PayloadLength = _payload.Length,
            Payload = payloadString,
            Signature = Convert.ToBase64String(Signature)
        };

        return System.Text.Json.JsonSerializer.Serialize(json);
    }
}