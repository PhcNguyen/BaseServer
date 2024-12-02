using NServer.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NServer.Core.BufferPool
{
    public class MultiSizeBuffer : IDisposable
    {
        private readonly Dictionary<int, double> _bufferAllocations = Setting.BufferAllocations;
        private readonly SortedDictionary<int, SharedBufferPool> _pools = []; // Khởi tạo đúng kiểu dữ liệu
        private readonly int _totalBuffers = Setting.TotalBuffers;
        private readonly object _initLock = new();
        private bool _isInitialized = false;
        private bool _disposed = false;

        private int _rentedCount = 0; // Số lần thuê bộ đệm
        private const int MaxRentsBeforeAdjustment = 10; // Sau mỗi 10 lần thuê, điều chỉnh pool

        public void AllocateBuffers()
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException("Buffers have already been allocated.");
            }

            lock (_initLock)
            {
                if (_isInitialized)
                {
                    throw new InvalidOperationException("Buffers have already been allocated.");
                }

                foreach (var allocation in _bufferAllocations)
                {
                    int bufferSize = allocation.Key;
                    int initialCapacity = (int)(_totalBuffers * allocation.Value);
                    _pools[bufferSize] = SharedBufferPool.GetOrCreatePool(bufferSize, initialCapacity);
                }

                _isInitialized = true;
            }
        }

        public byte[] RentBuffer(int size = 256)
        {
            var suitableBuffer = _pools.Keys.FirstOrDefault(key => key >= size);
            if (suitableBuffer == 0)
            {
                throw new ArgumentException("Requested buffer size exceeds maximum allowed size.");
            }

            var buffer = _pools[suitableBuffer].AcquireBuffer();

            // Không điều chỉnh khi thuê bộ đệm, chỉ đếm số lần thuê
            if (_rentedCount++ >= MaxRentsBeforeAdjustment)
            {
                if (_pools.TryGetValue(suitableBuffer, out var pool) && pool.FreeBuffers <= pool.TotalBuffers * 0.2)
                    // Nếu số lượng bộ đệm rảnh ít hơn ngưỡng tối thiểu, tăng bộ đệm
                    IncreaseBufferPoolSize(suitableBuffer, pool.TotalBuffers / 2).Wait();

                _rentedCount = 0; // Reset đếm sau mỗi 10 lần thuê
            }
            return buffer;
        }

        public void ReturnBuffer(byte[] buffer)
        {
            if (buffer == null) return;

            if (_pools.TryGetValue(buffer.Length, out var pool))
            {
                lock (_initLock)
                {
                    pool.ReleaseBuffer(buffer);
                    pool.GetInfo(out int free, out int total, out int size, out _);

                    // Tính toán điều chỉnh bộ đệm
                    double a = (int)(free - _bufferAllocations[size] * _totalBuffers);
                    if (a < 0) a = 0;
                    double b = (int)(total * 0.2);

                    if (a > b)
                    {
                        int buffersToShrink = (int)(a - b);
                        int maxShrinkable = (int)(_bufferAllocations[size] * _totalBuffers);

                        // Giảm số bộ đệm không vượt quá số bộ đệm mặc định
                        if (buffersToShrink > maxShrinkable)
                        {
                            buffersToShrink = maxShrinkable;
                        }

                        // Giảm bộ đệm nhưng không giảm quá mạnh
                        if (buffersToShrink > 20) // Giảm tối đa 10 bộ đệm mỗi lần
                        {
                            buffersToShrink = 20;
                        }

                        ShrinkBufferPoolSize(size, buffersToShrink);
                    }
                }
            }
            else
            {
                throw new ArgumentException("Invalid buffer size.");
            }
        }

        public async Task IncreaseBufferPoolSize(int bufferSize, int quantityToIncrease)
        {
            if (!_pools.TryGetValue(bufferSize, out SharedBufferPool? pool))
            {
                throw new ArgumentException("Buffer size not found.");
            }

            if (quantityToIncrease <= 0)
            {
                throw new ArgumentException("The number of buffers to increase must be greater than zero.");
            }

            // Tăng dung lượng bộ đệm
            await Task.Run(() => pool.IncreaseCapacity(quantityToIncrease));
        }

        public void ShrinkBufferPoolSize(int bufferSize, int numberOfBuffersToRemove)
        {
            if (!_pools.TryGetValue(bufferSize, out var pool))
            {
                throw new ArgumentException("Buffer size not found.");
            }

            if (numberOfBuffersToRemove <= 0 || numberOfBuffersToRemove > pool.FreeBuffers)
            {
                throw new ArgumentException("Invalid number of buffers to remove.");
            }

            // Giảm dung lượng bộ đệm
            pool.DecreaseCapacity(numberOfBuffersToRemove);
        }

        /// <summary>
        /// Lấy thông tin về pool.
        /// </summary>
        /// <param name="bufferSize">Kích thước bộ đệm.</param>
        /// <param name="freeCount">Số lượng bộ đệm còn lại.</param>
        /// <param name="totalBuffers">Tổng số bộ đệm.</param>
        /// <param name="bufferSizeOut">Kích thước của bộ đệm.</param>
        /// <param name="misses">Số lần thiếu hụt.</param>
        public void GetPoolInfo(int bufferSize, out int freeCount, out int totalBuffers, out int bufferSizeOut, out int misses)
        {
            if (_pools.TryGetValue(bufferSize, out var pool))
            {
                pool.GetInfo(out freeCount, out totalBuffers, out bufferSizeOut, out misses);
            }
            else
            {
                throw new ArgumentException("Buffer size not found.");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                if (_isInitialized)
                {
                    foreach (var pool in _pools.Values)
                    {
                        pool.Dispose();
                    }
                    _pools.Clear();
                }
                _isInitialized = false;
            }

            _disposed = true;
        }
    }
}