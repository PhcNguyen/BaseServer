using System;

using ToolClient.Core.Packets.Base;
using ToolClient.Core.Packets.Enums;
using ToolClient.Core.Packets.Metadata;

namespace ToolClient.Core.Packets
{
    /// <summary>
    /// Gói tin cơ bản, kế thừa từ PacketBase.
    /// </summary>
    internal class Packet : PacketBase
    {
        /// <summary>
        /// Constructor để tạo Packet với Command và Payload.
        /// </summary>
        public Packet(byte? type = null, byte? flags = null, short? command = null, byte[]? payload = null)
        {
            Type = type is not null && Enum.IsDefined((PacketType)type)
                    ? (PacketType)type
                    : PacketType.NONE;

            Flags = flags is not null && Enum.IsDefined((PacketFlags)flags)
                    ? (PacketFlags)flags
                    : PacketFlags.NONE;

            Cmd = command ?? 0;

            if (payload != null && payload.Length + PacketMetadata.HEADERSIZE > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(payload), "Payload quá lớn.");
            }

            Payload = payload?.Length > 0 ? new Memory<byte>(payload) : Memory<byte>.Empty;
        }
    }
}

