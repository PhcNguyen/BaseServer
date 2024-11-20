using NETServer.Application.Handlers;
using NETServer.Infrastructure.Configuration;
using NETServer.Core.Network.Packet.Enums;
using NETServer.Interfaces.Core.Network.Packet;

using System.Runtime.CompilerServices;

namespace NETServer.Core.Network.Packet
{
    /// <summary>
    /// Gói tin cơ bản, kế thừa từ PacketBase.
    /// </summary>
    internal class Packet : PacketBase, IPacket
    {
        /// <summary>
        /// Constructor để tạo Packet với Command và Payload.
        /// </summary>
        public Packet(Guid id, byte? version = null, byte? flags = null, short? command = null, byte[]? payload = null)
        {
            Id = id;
            Timestamp = DateTimeOffset.UtcNow;

            Version = version ?? Setting.VERSION;

            Flags = flags is not null && Enum.IsDefined((PacketFlags)flags)
                    ? (PacketFlags)flags
                    : PacketFlags.NONE;

            Command = command ?? (short)Cmd.NONE;

            Payload = payload?.Length > 0
                      ? new Memory<byte>(payload)
                      : Memory<byte>.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Packet FromByteArray(byte[] data)
        {
            if (data == null || data.Length < PacketMetadata.HEADERSIZE)
            {
                throw new ArgumentException("Invalid data length.", nameof(data));
            }

            // Chuyển mảng byte thành Span để xử lý dễ dàng hơn
            var span = data.AsSpan();

            // Đọc độ dài gói tin (Length) từ offset
            int length = BitConverter.ToInt32(span[..sizeof(int)]);
            if (length != data.Length -4)
            {
                throw new ArgumentException("Data length mismatch.", nameof(data));
            }

            // Khôi phục các giá trị từ gói tin
            Version = span[PacketMetadata.VERSIONOFFSET];
            Flags = (PacketFlags)span[PacketMetadata.FLAGSOFFSET];
            Command = BitConverter.ToInt16(span.Slice(PacketMetadata.COMMANDOFFSET, sizeof(short)));

            // Khôi phục payload
            Payload = span[PacketMetadata.HEADERSIZE..length].ToArray();

            return this;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValid()
        {
            if (Command == (short)Cmd.NONE)
            {
                return false;
                throw new InvalidOperationException("Invalid Command.");
            }

            if (Payload.Length == 0)
            {
                return false;
                throw new InvalidOperationException("Payload is empty.");
            }

            return true;
        }


        // Ví dụ: chỉ chênh lệch 1 phiên bản.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCompatible(byte otherVersion) =>
            Math.Abs(Version - otherVersion) <= 1;

        public int CompareTo(Packet? other)
        {
            if (other == null) return 1;

            int thisPriority = PacketExtensions.DeterminePriority(this);
            int otherPriority = PacketExtensions.DeterminePriority(other);

            int priorityComparison = otherPriority.CompareTo(thisPriority);
            return priorityComparison == 0
                ? Timestamp.CompareTo(other.Timestamp)
                : priorityComparison;
        }
    }
}

