using System.Runtime.CompilerServices;

namespace NETServer.Network.Packets
{
    internal struct PacketMetadata
    {
        // Các vị trí trong gói tin
        public const int LENGHTOFFSET = 0;   // 4 byte 
        public const int VERSIONOFFSET = 4;  // 1 byte
        public const int FLAGSOFFSET = 5;    // 1 byte
        public const int COMMANDOFFSET = 6;  // 2 byte

        public const int HEADERSIZE = sizeof(byte) * 2 + sizeof(short) + sizeof(int); //8
    }

    internal abstract class PacketBase
    {
        private static readonly PacketFlags[] CachedFlags =
            (PacketFlags[])Enum.GetValues(typeof(PacketFlags));

        public Guid? ID { get; protected set; }

        // Kích thước cố định của header.
        private const int HeaderSize = PacketMetadata.HEADERSIZE;

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
        public short? Command { get; protected set; }

        /// <summary>
        /// Payload của gói tin, chứa nội dung chính.
        /// </summary>
        public Memory<byte> Payload { get; protected set; }

        /// <summary>
        /// Tổng chiều dài gói tin (Header + Payload).
        /// </summary>
        public int Length => HeaderSize + (Payload.IsEmpty ? 0 : Payload.Length);

        /// <summary>
        /// Chuyển đổi gói tin thành byte array để gửi đi.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual byte[] ToByteArray()
        {
            var packet = new byte[Length];
            var span = packet.AsSpan();

            // Header.
            BitConverter.TryWriteBytes(span[PacketMetadata.LENGHTOFFSET..], Length);
            span[PacketMetadata.VERSIONOFFSET] = Version;
            span[PacketMetadata.FLAGSOFFSET] = (byte)Flags;
            BitConverter.TryWriteBytes(span[PacketMetadata.COMMANDOFFSET..], Command ?? short.MinValue);

            // Payload.
            Payload.Span.CopyTo(span[HeaderSize..]);

            return packet;
        }

        /// <summary>
        /// Lấy danh sách các cờ trạng thái đang được thiết lập (trả về dưới dạng IEnumerable).
        /// </summary>
        /// <param name="filter">Hàm lọc tùy chọn để kiểm tra các flag.</param>
        /// <returns>Danh sách các cờ trạng thái dưới dạng IEnumerable.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IEnumerable<PacketFlags> CombinedFlags(Func<PacketFlags, bool>? filter = null)
        {
            foreach (var flag in CachedFlags)
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