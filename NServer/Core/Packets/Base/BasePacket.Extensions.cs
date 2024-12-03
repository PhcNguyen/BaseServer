using System;
using System.Text;

namespace NServer.Core.Packets.Base;

public partial class BasePacket
{
    /// <summary>
    /// Phương thức để đặt lại gói tin về trạng thái ban đầu.
    /// </summary>
    public void Reset()
    {
        Flags = Enums.PacketFlags.NONE;
        Cmd = (short)0;
        Payload = Memory<byte>.Empty;
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