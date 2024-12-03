using System;
using System.Linq;
using System.Collections.Concurrent;

namespace NServer.Core.BufferPool
{
    public class BufferPoolManager
    {
        private readonly ConcurrentDictionary<int, SharedBufferPool> _pools = new();
        private readonly ConcurrentDictionary<int, int> _adjustmentCounters = new();
        private int[] _sortedKeys = [];

        public event Action<SharedBufferPool>? EventIncrease;
        public event Action<SharedBufferPool>? EventShrink;

        // Tạo pool
        public void CreatePool(int bufferSize, int initialCapacity)
        {
            if (_pools.TryAdd(bufferSize, SharedBufferPool.GetOrCreatePool(bufferSize, initialCapacity)))
            {
                // Cập nhật danh sách kích thước đã sắp xếp
                _sortedKeys = [.. _pools.Keys.OrderBy(k => k)];
            }
        }

        // Thuê buffer
        public byte[] RentBuffer(int size)
        {
            int poolSize = FindSuitablePoolSize(size);
            if (poolSize == 0)
                throw new ArgumentException("Requested buffer size exceeds maximum available pool size.");

            var pool = _pools[poolSize];
            var buffer = pool.AcquireBuffer();

            if (TriggerBufferAdjustment(poolSize, EventIncrease))
                EventIncrease?.Invoke(pool);

            return buffer;
        }

        // Trả buffer
        public void ReturnBuffer(byte[] buffer)
        {
            if (buffer == null || !_pools.TryGetValue(buffer.Length, out var pool))
                throw new ArgumentException("Invalid buffer size.");

            pool.ReleaseBuffer(buffer);

            if (TriggerBufferAdjustment(buffer.Length, EventShrink))
                EventShrink?.Invoke(pool);
        }

        // Tìm kích thước pool phù hợp
        private int FindSuitablePoolSize(int size)
        {
            foreach (var key in _sortedKeys)
            {
                if (key >= size)
                    return key;
            }
            return 0;
        }

        // Điều chỉnh buffer
        private bool TriggerBufferAdjustment(int poolSize, Action<SharedBufferPool>? eventAction)
        {
            var counter = _adjustmentCounters.AddOrUpdate(poolSize, 1, (_, current) => current + 1);
            if (counter >= 10)
            {
                _adjustmentCounters[poolSize] = 0;
                return true;
            }
            return false;
        }

        // Giải phóng tài nguyên
        public void Dispose()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Dispose();
            }
            _pools.Clear();
            _sortedKeys = [];
        }
    }
}