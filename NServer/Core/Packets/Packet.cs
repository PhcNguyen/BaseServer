using NServer.Core.Interfaces.Packets;
using NServer.Core.Packets.Base;
using NServer.Core.Packets.Enums;
using NServer.Core.Packets.Metadata;
using System;

namespace NServer.Core.Packets
{
    /// <summary>
    /// Gói tin cơ bản, kế thừa từ PacketBase.
    /// </summary>
    public class Packet : BasePacket, IPacket
    {
        /// <summary>
        /// Constructor để tạo Packet với Command và Payload.
        /// </summary>
        public Packet(byte? type = null, byte? flags = null, short? command = null, byte[]? payload = null)
        {
            Type = type is not null && Enum.IsDefined((PacketType)type)
                    ? (PacketType)type
                    : PacketType.NONE;

            Flags = flags is not null && Enum.IsDefined((Enums.PacketFlags)flags)
                    ? (Enums.PacketFlags)flags
                    : Enums.PacketFlags.NONE;

            Cmd = command ?? 0;

            if (payload != null && payload.Length + PacketMetadata.HEADERSIZE > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(payload), "The payload is too large.");
            }

            Payload = payload?.Length > 0 ? new Memory<byte>(payload) : Memory<byte>.Empty;
        }
    }
}