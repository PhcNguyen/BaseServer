using NETServer.Application.Handlers;
using NETServer.Core.Network.Packet;
using NETServer.Core.Network.Packet.Enums;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace NETServer.Core.Network.Packet
{
    internal abstract partial class PacketBase
    {
        private Memory<byte> _payload;
        private int _payloadLength;

        // Kích thước cố định của header.
        private const int _headerSize = PacketMetadata.HEADERSIZE;

        public Guid Id { get; protected set; }
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Phiên bản giao thức hoặc gói tin.
        /// </summary>
        public byte Version { get; protected set; } = 1;

        /// <summary>
        /// Cờ trạng thái.
        /// </summary>
        public PacketFlags Flags { get; protected set; } = PacketFlags.NONE;

        /// <summary>
        /// Command xác định loại gói tin.
        /// </summary>
        public short Command { get; protected set; } = (short)Cmd.NONE;

        /// <summary>
        /// Payload của gói tin, chứa nội dung chính.
        /// </summary>
        public Memory<byte> Payload
        {
            get => _payload;
            protected set
            {
                _payload = value;
                _payloadLength = _payload.Length;
            }
        }

        /// <summary>
        /// Tổng chiều dài gói tin (Header + Payload).
        /// </summary>
        public int Length => _headerSize + _payloadLength;

        /// <summary>
        /// Chuyển đổi gói tin thành byte array để gửi đi.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual byte[] ToByteArray()
        {
            byte[] packet = ArrayPool<byte>.Shared.Rent(Length);
            var span = packet.AsSpan(0, Length);

            // Header
            BitConverter.TryWriteBytes(span[PacketMetadata.LENGHTOFFSET..], Length);
            span[PacketMetadata.VERSIONOFFSET] = Version;
            span[PacketMetadata.FLAGSOFFSET] = (byte)Flags;
            BitConverter.TryWriteBytes(span[PacketMetadata.COMMANDOFFSET..], Command);

            // Payload
            Payload.Span.CopyTo(span[_headerSize..]);

            var result = packet[..Length].ToArray(); // Copy nội dung thực sự
            ArrayPool<byte>.Shared.Return(packet);

            return result;
        }

        /// <summary>
        /// Lấy danh sách các cờ trạng thái đang được thiết lập (trả về dưới dạng IEnumerable).
        /// </summary>
        /// <param name="filter">Hàm lọc tùy chọn để kiểm tra các flag.</param>
        /// <returns>Danh sách các cờ trạng thái dưới dạng IEnumerable.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IEnumerable<PacketFlags> CombinedFlags(Func<PacketFlags, bool>? filter = null)
        {
            foreach (var flag in Enum.GetValues<PacketFlags>())
            {
                if (flag != PacketFlags.NONE &&
                      (Flags & flag) == flag &&
                      (filter == null || filter(flag)))
                {
                    yield return flag;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() =>
            $"Packet [Version={Version}, Flags={Flags}, Command={Command}, PayloadSize={Payload.Length}]";
    }
}