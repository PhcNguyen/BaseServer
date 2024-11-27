using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

using NServer.Core.Interfaces.Packets;

namespace NServer.Core.Packets.Utils
{
    /// <summary>
    /// Lớp cơ sở cho các hàng đợi gói tin, cung cấp các phương thức chung để quản lý hàng đợi.
    /// </summary>
    internal abstract class BasePacketContainer : IDisposable
    {
        private readonly ConcurrentQueue<IPacket> _packetQueue = new();

        /// <summary>
        /// Thêm gói tin vào hàng đợi.
        /// </summary>
        /// <param name="packet">Gói tin cần thêm.</param>
        protected void EnqueuePacket(IPacket packet)
        {
            if (packet == null) throw new ArgumentNullException(nameof(packet), "Packet cannot be null.");
            _packetQueue.Enqueue(packet);
        }

        /// <summary>
        /// Lấy một gói tin tiếp theo từ hàng đợi để xử lý.
        /// </summary>
        /// <returns>Gói tin cần xử lý hoặc null nếu hàng đợi trống.</returns>
        public IPacket? DequeuePacket()
        {
            return _packetQueue.TryDequeue(out var packet) ? packet : null;
        }

        /// <summary>
        /// Lấy một lô gói tin từ hàng đợi để xử lý theo nhóm.
        /// </summary>
        /// <param name="batchSize">Số lượng gói tin cần lấy trong một lô.</param>
        /// <returns>Danh sách gói tin.</returns>
        public List<IPacket> DequeueBatch(int batchSize)
        {
            if (batchSize <= 0) throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be greater than zero.");

            var batch = new List<IPacket>(batchSize);
            while (batch.Count < batchSize && _packetQueue.TryDequeue(out var packet))
            {
                batch.Add(packet);
            }

            return batch;
        }

        /// <summary>
        /// Kiểm tra số lượng gói tin hiện tại trong hàng đợi.
        /// </summary>
        /// <returns>Số lượng gói tin trong hàng đợi.</returns>
        public int Count()
        {
            return _packetQueue.Count;
        }

        /// <summary>
        /// Giải phóng tài nguyên được sử dụng bởi <see cref="BasePacketContainer"/>.
        /// </summary>
        public virtual void Dispose()
        {
            while (_packetQueue.TryDequeue(out _)) { } // Xóa toàn bộ hàng đợi
            GC.SuppressFinalize(this);
        }
    }
}