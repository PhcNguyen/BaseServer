using System;

using NServer.Core.Packets.Enums;
using NServer.Core.Packets.Utils;
using NServer.Core.Interfaces.Packets;

namespace NServer.Core.Packets
{
    /// <summary>
    /// Gói tin cơ bản, kế thừa từ PacketBase.
    /// </summary>
    internal class Packet : BasePacket, IPacket
    {
        /// <summary>
        /// Constructor để tạo Packet với Command và Payload.
        /// </summary>
        public Packet(byte? flags = null, short? command = null, byte[]? payload = null)
        {
            Flags = flags is not null && Enum.IsDefined((PacketFlags)flags)
                    ? (PacketFlags)flags
                    : PacketFlags.NONE;

            Command = command ?? 0;

            if (payload != null && payload.Length + PacketMetadata.HEADERSIZE > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(payload), "Payload quá lớn.");
            }

            Payload = payload?.Length > 0 ? new Memory<byte>(payload) : Memory<byte>.Empty;
        }
    }
}

