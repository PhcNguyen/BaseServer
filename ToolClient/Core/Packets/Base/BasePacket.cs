using System;
using System.Buffers;

using ToolClient.Core.Packets.Metadata;

namespace ToolClient.Core.Packets.Base
{
    /// <summary>
    /// Lớp cơ sở cho tất cả các gói tin mạng.
    /// </summary>
    internal abstract partial class PacketBase
    {
        /// <summary>
        /// Tổng chiều dài của gói tin, bao gồm header và payload.
        /// </summary>
        public int Length => _headerSize + _payload.Length;


        /// <summary>
        /// Chuyển đổi gói tin thành mảng byte để gửi qua mạng.
        /// </summary>
        /// <returns>Mảng byte của gói tin.</returns>
        public virtual byte[] ToByteArray()
        {
            // Sử dụng ArrayPool để giảm chi phí bộ nhớ heap
            byte[] packet = ArrayPool<byte>.Shared.Rent(Length);

            try
            {
                Span<byte> span = packet.AsSpan(0, Length);

                // Header
                BitConverter.TryWriteBytes(span[..], Length); // Ghi chiều dài gói tin
                span[PacketMetadata.TYPEOFFSET] = (byte)Type;
                span[PacketMetadata.FLAGSOFFSET] = (byte)Flags;
                BitConverter.TryWriteBytes(span[PacketMetadata.COMMANDOFFSET..], Cmd);
                
                // Payload
                _payload.Span.CopyTo(span[PacketMetadata.PAYLOADOFFSET..]);

                return span[..Length].ToArray();
            }
            finally
            {
                // Đảm bảo trả lại bộ nhớ vào ArrayPool ngay cả khi có ngoại lệ
                ArrayPool<byte>.Shared.Return(packet);
            }
        }
    }
}