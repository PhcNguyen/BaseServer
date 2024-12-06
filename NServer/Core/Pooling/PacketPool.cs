using NPServer.Core.Interfaces.Packets;
using NPServer.Core.Interfaces.Pooling;
using NPServer.Core.Packets;
using System.Collections.Concurrent;

namespace NPServer.Core.Pooling
{
    /// <summary>
    /// Quản lý pool của các gói tin để tái sử dụng và tối ưu hóa bộ nhớ.
    /// </summary>
    public class PacketPool : IPacketPool
    {
        private readonly ConcurrentStack<IPacket> _pool = new();

        /// <summary>
        /// Khởi tạo một đối tượng <see cref="PacketPool"/> với dung lượng ban đầu được chỉ định.
        /// </summary>
        /// <param name="initialCapacity">Số lượng gói tin ban đầu để cấp phát.</param>
        public PacketPool(int initialCapacity)
        {
            for (int i = 0; i < initialCapacity; i++)
            {
                _pool.Push(new Packet());  // Khởi tạo các gói tin ban đầu
            }
        }

        /// <summary>
        /// Lấy gói tin từ pool (nếu có).
        /// </summary>
        /// <returns>Gói tin từ pool nếu có, hoặc tạo gói tin mới nếu pool hết.</returns>
        public IPacket RentPacket()
        {
            if (_pool.TryPop(out var packet))
            {
                return packet;
            }

            return new Packet();
        }

        /// <summary>
        /// Trả gói tin vào pool để tái sử dụng.
        /// </summary>
        /// <param name="packet">Gói tin cần trả vào pool.</param>
        public void ReturnPacket(IPacket packet)
        {
            packet.Reset();
            _pool.Push(packet);
        }

        /// <summary>
        /// Kiểm tra số lượng gói tin còn lại trong pool.
        /// </summary>
        public int Count => _pool.Count;
    }
}