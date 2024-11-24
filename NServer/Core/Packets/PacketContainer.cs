using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace NServer.Core.Packets
{
    /// <summary>
    /// Lớp quản lý hàng đợi gói tin toàn cục, xử lý gói tin nhanh và hiệu quả.
    /// </summary>
    internal class PacketContainer : IDisposable
    {
        // Hàng đợi gói tin toàn cục
        private readonly ConcurrentQueue<Packet> _packetQueue = new();

        /// <summary>
        /// Thêm gói tin vào hàng đợi.
        /// </summary>
        /// <param name="packet">Gói tin cần thêm.</param>
        public void AddPacket(Packet packet)
        {
            _packetQueue.Enqueue(packet);
        }

        /// <summary>
        /// Lấy một gói tin tiếp theo từ hàng đợi để xử lý.
        /// </summary>
        /// <returns>Gói tin cần xử lý hoặc null nếu hàng đợi trống.</returns>
        public Packet? GetNextPacket()
        {
            if (_packetQueue.TryDequeue(out var packet))
            {
                return packet;
            }
            return null; // Không có gói tin trong hàng đợi
        }

        /// <summary>
        /// Lấy một lô gói tin từ hàng đợi để xử lý theo nhóm.
        /// </summary>
        /// <param name="batchSize">Số lượng gói tin cần lấy trong một lô.</param>
        /// <returns>Danh sách gói tin.</returns>
        public List<Packet> GetPacketsBatch(int batchSize)
        {
            var batch = new List<Packet>(batchSize);
            while (batch.Count < batchSize && _packetQueue.TryDequeue(out var packet))
            {
                batch.Add(packet);
            }
            return batch;
        }

        /// <summary>
        /// Kiểm tra số lượng gói tin hiện tại trong hàng đợi.
        /// </summary>
        public int GetQueueLength()
        {
            return _packetQueue.Count;
        }

        /// <summary>
        /// Giải phóng tài nguyên được sử dụng bởi <see cref="PacketContainer"/>.
        /// </summary>
        public void Dispose()
        {
            while (_packetQueue.TryDequeue(out _)) { } // Xóa toàn bộ hàng đợi
            GC.SuppressFinalize(this);
        }
    }
}