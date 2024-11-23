using System;
using System.Threading;
using System.Collections.Concurrent;

namespace NServer.Core.Network.Buffers
{
    /// <summary>
    /// Lớp quản lý pool bộ đệm dùng chung.
    /// </summary>
    internal class SharedBufferPool : IDisposable
    {
        private static readonly ConcurrentDictionary<int, SharedBufferPool> GlobalPools = new();

        private readonly SemaphoreSlim _expandSemaphore = new(1, 1);
        private readonly ConcurrentQueue<byte[]> _freeBuffers;
        private readonly int _bufferSize;
        private readonly int _initialCapacity;
        private int _misses;

        /// <summary>
        /// Tổng số bộ đệm đã được cấp phát.
        /// </summary>
        public int TotalBuffers { get; private set; }

        /// <summary>
        /// Lấy số lượng bộ đệm còn lại trong pool.
        /// </summary>
        public int FreeBuffers => _freeBuffers.Count;

        private bool _disposed;

        /// <summary>
        /// Khởi tạo một <see cref="SharedBufferPool"/> với kích thước bộ đệm và dung lượng ban đầu.
        /// </summary>
        /// <param name="bufferSize">Kích thước của bộ đệm.</param>
        /// <param name="initialCapacity">Dung lượng ban đầu của pool.</param>
        private SharedBufferPool(int bufferSize, int initialCapacity)
        {
            _bufferSize = bufferSize;
            _initialCapacity = initialCapacity;
            _freeBuffers = new ConcurrentQueue<byte[]>();

            for (int i = 0; i < _initialCapacity; ++i)
            {
                _freeBuffers.Enqueue(new byte[bufferSize]);
            }

            TotalBuffers = initialCapacity;
        }

        /// <summary>
        /// Lấy hoặc tạo pool bộ đệm dùng chung theo kích thước bộ đệm.
        /// </summary>
        /// <param name="bufferSize">Kích thước của bộ đệm.</param>
        /// <param name="initialCapacity">Dung lượng ban đầu của pool.</param>
        /// <returns>Pool bộ đệm tương ứng.</returns>
        public static SharedBufferPool GetOrCreatePool(int bufferSize, int initialCapacity)
        {
            return GlobalPools.GetOrAdd(bufferSize, _ => new SharedBufferPool(bufferSize, initialCapacity));
        }

        /// <summary>
        /// Lấy một bộ đệm từ pool.
        /// </summary>
        /// <returns>Bộ đệm byte.</returns>
        public byte[] AcquireBuffer()
        {
            if (_freeBuffers.TryDequeue(out var buffer))
            {
                return buffer;
            }

            // Khi không có buffer, cố gắng mở rộng pool
            _expandSemaphore.Wait();
            try
            {
                _misses++;
                TotalBuffers++;
                return new byte[_bufferSize]; // Cấp phát bộ đệm mới
            }
            finally
            {
                _expandSemaphore.Release();
            }
        }

        /// <summary>
        /// Trả lại bộ đệm về pool.
        /// </summary>
        /// <param name="buffer">Bộ đệm cần trả lại.</param>
        public void ReleaseBuffer(byte[] buffer)
        {
            if (buffer == null || buffer.Length != _bufferSize)
            {
                return; // Không chấp nhận buffer sai kích thước.
            }

            _freeBuffers.Enqueue(buffer);
        }

        /// <summary>
        /// Phương thức tăng dung lượng pool.
        /// </summary>
        /// <param name="additionalCapacity">Số lượng bộ đệm cần thêm.</param>
        public void IncreaseCapacity(int additionalCapacity)
        {
            // Kiểm tra nếu dung lượng không quá lớn
            if (additionalCapacity <= 0)
            {
                throw new ArgumentException("Additional capacity must be greater than zero.");
            }

            _expandSemaphore.Wait();
            try
            {
                for (int i = 0; i < additionalCapacity; ++i)
                {
                    _freeBuffers.Enqueue(new byte[_bufferSize]);
                }
                TotalBuffers += additionalCapacity;
            }
            finally
            {
                _expandSemaphore.Release();
            }
        }

        /// <summary>
        /// Giảm dung lượng pool.
        /// </summary>
        /// <param name="capacityToRemove">Số lượng bộ đệm cần giảm.</param>
        public void DecreaseCapacity(int capacityToRemove)
        {
            lock (this)
            {
                // Chỉ giảm nếu có đủ bộ đệm trong pool
                for (int i = 0; i < capacityToRemove; ++i)
                {
                    if (_freeBuffers.TryDequeue(out _))  // Lấy ra một bộ đệm để xóa
                    {
                        TotalBuffers--;
                    }
                    else
                    {
                        break;  // Nếu không còn bộ đệm nào để xóa, dừng lại
                    }
                }
            }
        }

        /// <summary>
        /// Lấy thông tin về pool.
        /// </summary>
        /// <param name="freeCount">Số lượng bộ đệm còn lại.</param>
        /// <param name="totalBuffers">Tổng số bộ đệm.</param>
        /// <param name="bufferSize">Kích thước của bộ đệm.</param>
        /// <param name="misses">Số lần thiếu hụt.</param>
        public void GetInfo(out int freeCount, out int totalBuffers, out int bufferSize, out int misses)
        {
            freeCount = FreeBuffers;
            totalBuffers = TotalBuffers;
            bufferSize = _bufferSize;
            misses = _misses;
        }

        /// <summary>
        /// Giải phóng tài nguyên.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                while (_freeBuffers.TryDequeue(out _)) { }

                GlobalPools.TryRemove(_bufferSize, out _);
            }
        }
    }
}
