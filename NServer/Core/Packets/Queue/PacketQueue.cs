using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

using NServer.Core.Interfaces.Packets;

namespace NServer.Core.Packets.Queue
{
    /// <summary>
    /// Lớp cơ sở cho các hàng đợi gói tin, cung cấp các phương thức chung để quản lý hàng đợi.
    /// </summary>
    internal abstract class PacketQueue : IDisposable, IPacketQueue
    {
        private readonly ConcurrentQueue<IPacket> _queue = new();

        /// <summary>
        /// Thêm gói tin vào hàng đợi.
        /// </summary>
        /// <param name="packet">Gói tin cần thêm.</param>
        protected void Enqueue(IPacket packet)
        {
            if (packet == null) throw new ArgumentNullException(nameof(packet), "Packet cannot be null.");
            _queue.Enqueue(packet);
        }

        /// <summary>
        /// Lấy một gói tin tiếp theo từ hàng đợi để xử lý.
        /// </summary>
        /// <returns>Gói tin cần xử lý hoặc null nếu hàng đợi trống.</returns>
        public IPacket? Dequeue()
        {
            return _queue.TryDequeue(out var packet) ? packet : null;
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
            while (batch.Count < batchSize && _queue.TryDequeue(out var packet))
            {
                batch.Add(packet);
            }

            return batch;
        }

        /// <summary>
        /// Lấy gói tin đầu tiên từ hàng đợi mà không xóa nó.
        /// </summary>
        /// <returns>Gói tin đầu tiên trong hàng đợi hoặc null nếu hàng đợi trống.</returns>
        public IPacket? Peek()
        {
            return _queue.TryPeek(out var packet) ? packet : null;
        }

        /// <summary>
        /// Kiểm tra xem hàng đợi có chứa một gói tin cụ thể hay không.
        /// </summary>
        /// <param name="packet">Gói tin cần kiểm tra.</param>
        /// <returns>True nếu hàng đợi chứa gói tin, ngược lại False.</returns>
        public bool Contains(IPacket packet)
        {
            return _queue.Contains(packet);
        }

        /// <summary>
        /// Xóa tất cả các gói tin trong hàng đợi.
        /// </summary>
        public void Clear()
        {
            while (_queue.TryDequeue(out _)) { }
        }

        /// <summary>
        /// Chuyển đổi hàng đợi thành danh sách.
        /// </summary>
        /// <returns>Danh sách chứa các gói tin trong hàng đợi.</returns>
        public List<IPacket> ToList()
        {
            return [.. _queue];
        }

        /// <summary>
        /// Kiểm tra số lượng gói tin hiện tại trong hàng đợi.
        /// </summary>
        /// <returns>Số lượng gói tin trong hàng đợi.</returns>
        public int Count()
        {
            return _queue.Count;
        }

        /// <summary>
        /// Giải phóng tài nguyên được sử dụng bởi <see cref="PacketQueue"/>.
        /// </summary>
        public virtual void Dispose()
        {
            Clear(); // Xóa toàn bộ hàng đợi
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Lọc các gói tin dựa trên điều kiện cho trước.
        /// </summary>
        /// <param name="predicate">Điều kiện để lọc gói tin.</param>
        /// <returns>Danh sách các gói tin phù hợp với điều kiện.</returns>
        public IEnumerable<IPacket> Filter(Func<IPacket, bool> predicate)
        {
            return _queue.Where(predicate);
        }
    }
}