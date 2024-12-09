using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace NPServer.Infrastructure.Collections
{
    /// <summary>
    /// Lớp cơ sở cho các hàng đợi gói tin, cung cấp các phương thức chung để quản lý hàng đợi.
    /// </summary>
    public abstract class CustomQueues<TClass> : IDisposable where TClass : class
    {
        private readonly ConcurrentQueue<TClass> _queue = new();
        private readonly SemaphoreSlim _semaphore = new(0); // Hỗ trợ chờ tín hiệu mới.
        private bool _disposed;

        /// <summary>
        /// Sự kiện được kích hoạt khi có gói tin mới được thêm vào hàng đợi.
        /// </summary>
        public event Action? PacketAdded;

        /// <summary>
        /// Thêm gói tin vào hàng đợi.
        /// </summary>
        public void Enqueue(TClass packet)
        {
            if (packet == null) throw new ArgumentNullException(nameof(packet), "Packet cannot be null.");
            _queue.Enqueue(packet);

            // Giải phóng tín hiệu và kích hoạt sự kiện
            _semaphore.Release();
            PacketAdded?.Invoke();
        }

        /// <summary>
        /// Lấy một gói tin tiếp theo từ hàng đợi để xử lý.
        /// </summary>
        public TClass? Dequeue()
        {
            return _queue.TryDequeue(out TClass? packet) ? packet : null;
        }

        /// <summary>
        /// Lấy một lô gói tin từ hàng đợi để xử lý theo nhóm.
        /// </summary>
        public List<TClass> DequeueBatch(int batchSize)
        {
            if (batchSize <= 0) throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be greater than zero.");
            var batch = new List<TClass>(batchSize);

            while (batch.Count < batchSize && _queue.TryDequeue(out var packet))
            {
                batch.Add(packet);
            }

            return batch;
        }

        /// <summary>
        /// Lấy gói tin theo nhóm không đồng bộ, hỗ trợ chờ tín hiệu mới.
        /// </summary>
        public async Task<List<TClass>> DequeueBatchAsync(int batchSize, CancellationToken cancellationToken = default)
        {
            if (batchSize <= 0) throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be greater than zero.");
            var batch = new List<TClass>(batchSize);

            while (batch.Count < batchSize && !cancellationToken.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var packet))
                {
                    batch.Add(packet);
                }
                else
                {
                    await Task.WhenAny(_semaphore.WaitAsync(cancellationToken), Task.Delay(10, cancellationToken));
                }
            }

            return batch;
        }

        /// <summary>
        /// Lấy gói tin đầu tiên từ hàng đợi mà không xóa nó.
        /// </summary>
        public TClass? Peek()
        {
            return _queue.TryPeek(out var packet) ? packet : null;
        }

        /// <summary>
        /// Kiểm tra xem hàng đợi có chứa một gói tin cụ thể hay không.
        /// </summary>
        public bool Contains(TClass packet)
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
        public List<TClass> ToList()
        {
            return [.. _queue];
        }

        /// <summary>
        /// Kiểm tra số lượng gói tin hiện tại trong hàng đợi.
        /// </summary>
        public int Count => _queue.Count;

        /// <summary>
        /// Hủy bỏ một gói tin cụ thể từ hàng đợi nếu có.
        /// </summary>
        public bool TryRemove(TClass packet)
        {
            var tempQueue = new ConcurrentQueue<TClass>();
            bool removed = false;

            while (_queue.TryDequeue(out var currentPacket))
            {
                if (EqualityComparer<TClass>.Default.Equals(currentPacket, packet))
                {
                    removed = true;
                    continue;
                }
                tempQueue.Enqueue(currentPacket);
            }

            while (tempQueue.TryDequeue(out var remainingPacket))
            {
                _queue.Enqueue(remainingPacket);
            }

            return removed;
        }

        /// <summary>
        /// Lấy gói tin đầu tiên từ hàng đợi mà không xóa nó và hủy bỏ nếu có tín hiệu.
        /// </summary>
        public async Task<TClass?> PeekAsync(CancellationToken cancellationToken = default)
        {
            if (_queue.TryPeek(out var packet)) return packet;

            // Chờ tín hiệu mới khi hàng đợi rỗng
            await Task.WhenAny(_semaphore.WaitAsync(cancellationToken), Task.Delay(10, cancellationToken));
            return _queue.TryPeek(out var result) ? result : null;
        }

        /// <summary>
        /// Giải phóng tài nguyên được sử dụng bởi <see cref="CustomQueues"/>.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                Clear();
                PacketAdded = null;
                _semaphore.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}