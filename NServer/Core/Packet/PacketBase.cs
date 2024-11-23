using System;
using System.Buffers;

using NServer.Core.Packet.Enums;
using NServer.Core.Packet.Utils;

namespace NServer.Core.Packet
{
    /// <summary>
    /// Lớp cơ sở cho tất cả các gói tin mạng.
    /// </summary>
    internal abstract partial class PacketBase
    {
        private Memory<byte> _payload;
        private int _payloadLength;

        // Kích thước cố định của header.
        private const int _headerSize = PacketMetadata.HEADERSIZE;

        /// <summary>
        /// Thời gian tạo gói tin.
        /// </summary>
        public readonly DateTimeOffset Timestamp = DateTimeOffset.UtcNow;

        /// <summary>
        /// Cờ trạng thái của gói tin.
        /// </summary>
        public PacketFlags Flags { get; protected set; } = PacketFlags.NONE;

        /// <summary>
        /// Command để xác định loại gói tin.
        /// </summary>
        public short Command { get; protected set; } = 0;

        /// <summary>
        /// Dữ liệu chính của gói tin.
        /// </summary>
        public Memory<byte> Payload
        {
            get => _payload;
            protected set
            {
                if (value.Length > int.MaxValue - _headerSize)
                    throw new ArgumentOutOfRangeException(nameof(value), "Payload quá lớn.");
                _payload = value;
                _payloadLength = _payload.Length;
            }
        }

        /// <summary>
        /// Tổng chiều dài của gói tin, bao gồm header và payload.
        /// </summary>
        public int Length => _headerSize + _payloadLength;

        /// <summary>
        /// Chuyển đổi gói tin thành mảng byte để gửi qua mạng.
        /// </summary>
        /// <returns>Mảng byte của gói tin.</returns>
        public virtual byte[] ToByteArray()
        {
            byte[] packet = ArrayPool<byte>.Shared.Rent(Length);

            try
            {
                var span = packet.AsSpan(0, Length);

                // Header
                BitConverter.TryWriteBytes(span[PacketMetadata.LENGHTOFFSET..], Length);
                span[PacketMetadata.FLAGSOFFSET] = (byte)Flags;
                BitConverter.TryWriteBytes(span[PacketMetadata.COMMANDOFFSET..], Command);

                // Payload
                Payload.Span.CopyTo(span[_headerSize..]);

                return span.ToArray();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(packet);
            }
        }
    }
}