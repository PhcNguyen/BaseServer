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
    public int Length => _headerSize + _payload.Length + _signature.Length;

    /// <summary>
    /// Chuyển đổi gói tin thành mảng byte để gửi qua mạng.
    /// </summary>
    /// <returns>Mảng byte của gói tin.</returns>
    public byte[] ToByteArray()
    {
        this.SignPacket(); // Ký gói tin trước khi chuyển đổi

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

            if (_signature.Length > (Length - PacketMetadata.PAYLOADOFFSET - _payload.Length))
                throw new InvalidOperationException("Signature length exceeds allocated space.");

            // Copy signature
            _signature.CopyTo(span[(Length - _signature.Length)..]);

            return span[..(Length)].ToArray();
        }
        finally
        {
            // Đảm bảo trả lại bộ nhớ vào ArrayPool ngay cả khi có ngoại lệ
            Array.Clear(packet, 0, Length);
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

            // _signature
            _signature = data[length..].ToArray();

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
        var json = new
        {
            Flags,
            Cmd,
            PayloadLength = _payload.Length,
            Payload = _payload.Length > 0 ? Convert.ToBase64String(PayloadData.ToArray()) : null,
            Signature = _signature.Length > 0 ? Convert.ToBase64String(_signature) : null
        };

        return System.Text.Json.JsonSerializer.Serialize(json);
    }
}