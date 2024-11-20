using NETServer.Infrastructure.Configuration;
using System.Collections.Concurrent;

namespace NETServer.Core.Network.Buffers
{
    /// <summary>
    /// Lớp quản lý các pool bộ đệm với nhiều kích thước khác nhau.
    /// </summary>
    internal class MultiSizeBuffer
    {
        private readonly Dictionary<int, double> _bufferAllocations = Setting.BufferAllocations;
        private readonly ConcurrentDictionary<int, SharedBufferPool> _pools = new();
        private readonly int _totalBuffers = Setting.TotalBuffers;
        private bool _isInitialized = false; // Đánh dấu việc phân bổ đã được thực hiện

        /// <summary>
        /// Phân bổ bộ đệm lần đầu.
        /// </summary>
        public void AllocateBuffers()
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

            _isInitialized = true; // Đánh dấu đã khởi tạo xong
        }

        /// <summary>
        /// Thuê một bộ đệm với kích thước chỉ định.
        /// </summary>
        /// <param name="size">Kích thước bộ đệm yêu cầu.</param>
        /// <returns>Bộ đệm byte phù hợp.</returns>
        public byte[] RentBuffer(int size)
        {
            // Tìm kích thước bộ đệm gần nhất bằng Binary Search.
            foreach (var kvp in _pools)
            {
                if (size <= kvp.Key)
                {
                    var pool = kvp.Value;
                    var buffer = pool.AcquireBuffer();
                    if (pool.FreeBuffers < 10)
                    {
                        _ = IncreaseBufferPoolSize(kvp.Key);
                    }
                    return buffer;
                }
            }

            throw new ArgumentException("Requested buffer size exceeds maximum allowed size.");
        }

        /// <summary>
        /// Trả lại bộ đệm về pool.
        /// </summary>
        /// <param name="buffer">Bộ đệm cần trả lại.</param>
        public void ReturnBuffer(byte[] buffer)
        {
            if (buffer == null) return;

            if (_pools.TryGetValue(buffer.Length, out var pool))
            {
                pool.ReleaseBuffer(buffer);
            }
            else
            {
                throw new ArgumentException("Invalid buffer size.");
            }
        }

        /// <summary>
        /// Điều chỉnh tỷ lệ phân bổ khi tài nguyên thay đổi.
        /// </summary>
        /// <param name="bufferSize">Kích thước bộ đệm.</param>
        /// <param name="newPercentage">Tỷ lệ phân bổ mới.</param>
        public async Task AdjustBufferAllocationAsync(int bufferSize, double newPercentage)
        {
            if (_bufferAllocations.ContainsKey(bufferSize))
            {
                _bufferAllocations[bufferSize] = newPercentage;
                await ReallocateBuffersAsync();
            }
            else
            {
                throw new ArgumentException("Invalid buffer size.");
            }
        }

        /// <summary>
        /// Phân bổ lại bộ đệm theo tỷ lệ phân bổ mới.
        /// </summary>
        private async Task ReallocateBuffersAsync()
        {
            _pools.Clear();
            AllocateBuffers();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Tăng kích thước pool bộ đệm khi cần thiết.
        /// </summary>
        /// <param name="bufferSize">Kích thước bộ đệm cần tăng.</param>
        private async Task IncreaseBufferPoolSize(int bufferSize)
        {
            if (_pools.TryGetValue(bufferSize, out SharedBufferPool? value))
            {
                int newCapacity = (int)(_totalBuffers * _bufferAllocations[bufferSize]);
                await Task.Run(() => value.IncreaseCapacity(newCapacity));
            }
        }

        /// <summary>
        /// Điều chỉnh bộ đệm theo mức sử dụng.
        /// </summary>
        public void AdjustBufferAllocationBasedOnUsage()
        {
            foreach (var bufferSize in _pools.Keys)
            {
                var pool = _pools[bufferSize];
                if (pool.FreeBuffers > pool.TotalBuffers * 0.8)
                {
                    ShrinkBufferPoolSize(bufferSize);
                }
            }
        }

        /// <summary>
        /// Giảm kích thước pool bộ đệm.
        /// </summary>
        /// <param name="bufferSize">Kích thước bộ đệm cần giảm.</param>
        private void ShrinkBufferPoolSize(int bufferSize)
        {
            if (_pools.TryGetValue(bufferSize, out SharedBufferPool? value))
            {
                int newCapacity = (int)(_totalBuffers * _bufferAllocations[bufferSize] * 0.2);
                value.DecreaseCapacity(newCapacity);
            }
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

        /// <summary>
        /// Dọn dẹp các pool bộ đệm.
        /// </summary>
        public void Dispose()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Dispose();
            }
            _pools.Clear();
            _isInitialized = false;
        }
    }
}