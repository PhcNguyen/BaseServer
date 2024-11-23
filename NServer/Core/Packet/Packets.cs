using System;

using NServer.Core.Packet.Enums;
using NServer.Core.Packet.Utils;

namespace NServer.Core.Packet
{
    /// <summary>
    /// Gói tin cơ bản, kế thừa từ PacketBase.
    /// </summary>
    internal class Packets : PacketBase
    {
        /// <summary>
        /// Constructor để tạo Packet với Command và Payload.
        /// </summary>
        public Packets(byte? flags = null, short? command = null, byte[]? payload = null)
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

