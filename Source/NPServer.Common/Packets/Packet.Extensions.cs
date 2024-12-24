using NPServer.Common.Interfaces.Packets;
using NPServer.Common.Packets.Metadata;
using System;
using System.Buffers;

namespace NPServer.Common.Packets;

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
    public byte[] Pack()
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
    public bool UnPack(ReadOnlySpan<byte> data)
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
            this.Type = (PacketType)data[PacketMetadata.TYPEOFFSET];
            this.Flags = (PacketFlags)data[PacketMetadata.FLAGSOFFSET];
            this.Cmd = BitConverter.ToInt16(data[PacketMetadata.COMMANDOFFSET..]);

            // Payload
            this.PayloadData = data[PacketMetadata.PAYLOADOFFSET..length].ToArray();

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

    /// <summary>
    /// Kiểm tra sự tương đương giữa hai gói tin.
    /// </summary>
    /// <param name="obj">Gói tin để so sánh.</param>
    /// <returns>True nếu tương đương, ngược lại False.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is Packet otherPacket)
        {
            return Flags == otherPacket.Flags &&
                   Cmd == otherPacket.Cmd &&
                   Equals(this.PayloadData, otherPacket.PayloadData) &&
                   Equals(_signature, otherPacket._signature);
        }
        return false;
    }

    /// <summary>
    /// Tính mã băm bằng cách kết hợp các thành phần chính của gói tin.
    /// </summary>
    public override int GetHashCode()
    {
        int hashCode = HashCode.Combine(Flags, Cmd, _payload.Length, _signature.Length);
        return hashCode;
    }

    /// <summary>
    /// Tạo một bản sao của gói tin hiện tại.
    /// </summary>
    /// <returns>Bản sao của gói tin.</returns>
    public IPacket Clone()
    {
        var newPacket = new Packet
        {
            Flags = this.Flags,
            Cmd = this.Cmd,
            PayloadData = this.PayloadData.ToArray(),
            _signature = [.. this._signature]
        };
        return newPacket;
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của chữ ký (signature) của gói tin.
    /// </summary>
    /// <returns>True nếu chữ ký hợp lệ, ngược lại False.</returns>
    public bool ValidateSignature()
    {
        return VerifySignature();
    }
}