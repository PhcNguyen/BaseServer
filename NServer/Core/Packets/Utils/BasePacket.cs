using System;
using System.Buffers;
using NServer.Core.Packets.Enums;
using NServer.Core.Packets.Utils;
using NServer.Infrastructure.Services;

namespace NServer.Core.Packets
{
    /// <summary>
    /// Lớp cơ sở cho tất cả các gói tin mạng.
    /// </summary>
    internal abstract partial class BasePacket
    {
        private Memory<byte> _payload;
        private int _payloadLength;

        // Kích thước cố định của header.
        private const int _headerSize = PacketMetadata.HEADERSIZE;

        /// <summary>
        /// Thời gian tạo gói tin.
        /// </summary>
        public readonly DateTimeOffset Timestamp = DateTimeOffset.UtcNow;

        public ID36 Id { get; protected set; }

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
                    throw new ArgumentOutOfRangeException(nameof(value), "Payload too big.");
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
            // Tính toán chiều dài gói tin, bao gồm cả phần checksum
            int totalLength = Length + PacketMetadata.CHECKSUMSIZE;

            byte[] packet = ArrayPool<byte>.Shared.Rent(Length);

            try
            {
                var span = packet.AsSpan(0, totalLength);

                // Header
                BitConverter.TryWriteBytes(span[..], Length); // Ghi chiều dài gói tin
                span[PacketMetadata.FLAGSOFFSET] = (byte)Flags;
                BitConverter.TryWriteBytes(span[PacketMetadata.COMMANDOFFSET..], Command);

                // Payload
                Payload.Span.CopyTo(span[PacketMetadata.PAYLOADOFFSET..]);

                int checksum = PacketExtensions.CalculateChecksum(span[..(totalLength - PacketMetadata.CHECKSUMSIZE)].ToArray());
                BitConverter.TryWriteBytes(span[(totalLength - PacketMetadata.CHECKSUMSIZE)..], checksum);

                return span.ToArray();
            }
            finally
            {
                // Đảm bảo trả lại bộ nhớ vào ArrayPool ngay cả khi có ngoại lệ
                ArrayPool<byte>.Shared.Return(packet);
            }
        }
    }
}
