using NServer.Core.Packet.Enums;
using System.Text;

namespace NServer.Core.Packet.Utils
{
    /// <summary>
    /// Cung cấp các tiện ích mở rộng cho việc xử lý gói tin.
    /// </summary>
    internal static class PacketExtensions
    {
        /// <summary>
        /// Tạo một gói tin từ mảng byte.
        /// </summary>
        /// <param name="data">Mảng byte chứa dữ liệu gói tin.</param>
        /// <returns>Đối tượng <see cref="Packets"/> được tạo từ dữ liệu.</returns>
        /// <exception cref="ArgumentException">Nếu dữ liệu không hợp lệ.</exception>
        public static Packets FromByteArray(byte[] data)
        {
            if (data == null || data.Length < PacketMetadata.HEADERSIZE)
            {
                throw new ArgumentException("Invalid data length.", nameof(data));
            }

            var span = data.AsSpan();

            // Đọc Length và kiểm tra
            int length = BitConverter.ToInt32(span[0..sizeof(int)]);
            if (length > data.Length || length < PacketMetadata.HEADERSIZE)
            {
                throw new ArgumentException("Invalid packet length.", nameof(data));
            }

            byte flags = span[PacketMetadata.FLAGSOFFSET];
            short command = BitConverter.ToInt16(span[PacketMetadata.COMMANDOFFSET..]);
            byte[] payload = span[(PacketMetadata.HEADERSIZE + 1)..length].ToArray();

            // Tạo Packet từ dữ liệu
            return new Packets(flags, command, payload);    
        }

        /// <summary>
        /// Lấy chiều dài của gói tin từ header.
        /// </summary>
        /// <param name="packet">Mảng byte của gói tin.</param>
        /// <returns>Chiều dài của gói tin.</returns>
        public static int GetPacketLength(byte[] packet)
        {
            return BitConverter.ToInt32(packet, PacketMetadata.LENGHTOFFSET);
        }

        /// <summary>
        /// Lấy cờ (flags) từ gói tin.
        /// </summary>
        /// <param name="packet">Mảng byte của gói tin.</param>
        /// <returns>Giá trị cờ (flags).</returns>
        public static byte GetPacketFlags(byte[] packet)
        {
            return packet[PacketMetadata.FLAGSOFFSET];
        }

        /// <summary>
        /// Lấy lệnh (command) từ gói tin.
        /// </summary>
        /// <param name="packet">Mảng byte của gói tin.</param>
        /// <returns>Giá trị lệnh (command).</returns>
        public static short GetPacketCommand(byte[] packet)
        {
            return BitConverter.ToInt16(packet, PacketMetadata.COMMANDOFFSET);
        }

        /// <summary>
        /// Kiểm tra xem gói tin có hợp lệ hay không.
        /// </summary>
        /// <param name="packet">Mảng byte của gói tin.</param>
        /// <returns>True nếu hợp lệ, ngược lại là False.</returns>
        public static bool IsValidPacket(byte[] packet)
        {
            if (packet.Length < PacketMetadata.HEADERSIZE) return false;

            int length = BitConverter.ToInt32(packet, PacketMetadata.LENGHTOFFSET);
            return length > 0 && length <= packet.Length - PacketMetadata.HEADERSIZE;
        }

        /// <summary>
        /// Xác định độ ưu tiên của gói tin dựa trên cờ (flags).
        /// </summary>
        /// <param name="packet">Gói tin chứa cờ.</param>
        /// <returns>Mức độ ưu tiên (4: cao nhất, 0: không ưu tiên).</returns>
        public static int DeterminePriority(Packets packet) => packet.Flags switch
        {
            PacketFlags f when f.HasFlag(PacketFlags.ISURGENT) => 4, // Khẩn cấp
            PacketFlags f when f.HasFlag(PacketFlags.HIGH) => 3,     // Cao
            PacketFlags f when f.HasFlag(PacketFlags.MEDIUM) => 2,   // Trung bình
            PacketFlags f when f.HasFlag(PacketFlags.LOW) => 1,      // Thấp
            _ => 0,                                                  // Không có cờ ưu tiên nào
        };

        /// <summary>
        /// Lấy danh sách các cờ trạng thái hiện tại của gói tin.
        /// </summary>
        /// <param name="filter">
        /// Tùy chọn bộ lọc để kiểm tra các cờ (null để không lọc).
        /// </param>
        /// <returns>
        /// Danh sách các cờ trạng thái được thiết lập.
        /// </returns>
        public static IEnumerable<PacketFlags> CombinedFlags(this Packets packet, Func<PacketFlags, bool>? filter = null)
        {
            for (int i = 0; i < sizeof(PacketFlags) * 8; i++)
            {
                var flag = (PacketFlags)(1 << i);
                if ((packet.Flags & flag) == flag && (filter == null || filter(flag)))
                {
                    yield return flag;
                }
            }
        }
    }
}
