using System.Runtime.CompilerServices;

namespace NETServer.Network.Packets
{
    /// <summary>
    /// Quản lý nhiều gói tin trong một session hoặc kết nối.
    /// </summary>
    internal class PacketContainer
    {
        private readonly List<Packet> _packets = [];
        public IReadOnlyList<Packet> Packets => _packets.AsReadOnly();

        /// <summary>
        /// Thêm gói tin vào container.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPacket(Packet packet)
        {
            ArgumentNullException.ThrowIfNull(packet, nameof(packet));

            // Thêm gói tin vào danh sách.
            _packets.Add(packet);
            // Sắp xếp lại danh sách gói tin theo ưu tiên.
            _packets.Sort((p1, p2) => DeterminePriority(p2) - DeterminePriority(p1)); // Sắp xếp giảm dần (high -> low)
        }

        /// <summary>
        /// Lấy và xóa gói tin đầu tiên trong container theo ưu tiên.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DequeuePacket(out Packet? packet)
        {
            if (_packets.Count == 0)
            {
                packet = null;
                return false;
            }

            packet = _packets[0];
            _packets.RemoveAt(0);
            return true;
        }


        /// <summary>
        /// Lấy ưu tiên của gói tin dựa trên cờ trạng thái.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int DeterminePriority(Packet packet) => packet.Flags switch
        {
            PacketFlags f when f.HasFlag(PacketFlags.ISURGENT) => 4,// Khẩn cấp
            PacketFlags f when f.HasFlag(PacketFlags.HIGH) => 3,    // Cao
            PacketFlags f when f.HasFlag(PacketFlags.MEDIUM) => 2,  // Trung bình
            PacketFlags f when f.HasFlag(PacketFlags.LOW) => 1,     // Thấp
            _ => 0,                                                 // Không có cờ ưu tiên nào
        };
    }
}