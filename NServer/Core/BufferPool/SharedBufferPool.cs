using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace NServer.Core.BufferPool
{
    /// <summary>
    /// Quản lý một pool của các bộ đệm dùng chung.
    /// </summary>
    public sealed class SharedBufferPool : IDisposable
    {
        private static readonly ConcurrentDictionary<int, SharedBufferPool> GlobalPools = new();
        private readonly ConcurrentQueue<byte[]> _freeBuffers;
        private readonly ArrayPool<byte> _arrayPool;
        private readonly int _bufferSize;
        private int _totalBuffers;
        private bool _disposed;
        private int _misses;
        private readonly object _disposeLock = new();

        /// <summary>
        /// Tổng số lượng bộ đệm trong pool.
        /// </summary>
        public int TotalBuffers => _totalBuffers;

        /// <summary>
        /// Số lượng bộ đệm rảnh trong pool.
        /// </summary>
        public int FreeBuffers => _freeBuffers.Count;

        /// <summary>
        /// Khởi tạo một thể hiện mới của lớp <see cref="SharedBufferPool"/>.
        /// </summary>
        /// <param name="bufferSize">Kích thước của mỗi bộ đệm trong pool.</param>
        /// <param name="initialCapacity">Số lượng bộ đệm ban đầu để cấp phát.</param>
        private SharedBufferPool(int bufferSize, int initialCapacity)
        {
            _bufferSize = bufferSize;
            _arrayPool = ArrayPool<byte>.Shared;
            _freeBuffers = new ConcurrentQueue<byte[]>();

            for (int i = 0; i < initialCapacity; ++i)
            {
                _freeBuffers.Enqueue(_arrayPool.Rent(bufferSize));
            }

            _totalBuffers = initialCapacity;
        }

        /// <summary>
        /// Lấy hoặc tạo một pool bộ đệm chung cho kích thước bộ đệm chỉ định.
        /// </summary>
        /// <param name="bufferSize">Kích thước của mỗi bộ đệm trong pool.</param>
        /// <param name="initialCapacity">Số lượng bộ đệm ban đầu để cấp phát.</param>
        /// <returns>Đối tượng <see cref="SharedBufferPool"/> cho kích thước bộ đệm chỉ định.</returns>
        public static SharedBufferPool GetOrCreatePool(int bufferSize, int initialCapacity)
        {
            if (!GlobalPools.TryGetValue(bufferSize, out var pool))
            {
                lock (GlobalPools) // Đảm bảo thread safety khi truy cập từ nhiều luồng
                {
                    if (!GlobalPools.TryGetValue(bufferSize, out pool))
                    {
                        pool = new SharedBufferPool(bufferSize, initialCapacity);
                        GlobalPools[bufferSize] = pool;
                    }
                }
            }
            return pool;
        }

        /// <summary>
        /// Lấy một bộ đệm từ pool.
        /// </summary>
        /// <returns>Một mảng byte của bộ đệm.</returns>
        public byte[] AcquireBuffer()
        {
            if (_freeBuffers.TryDequeue(out var buffer))
            {
                return buffer;
            }

            Interlocked.Increment(ref _misses);
            Interlocked.Increment(ref _totalBuffers);

            return _arrayPool.Rent(_bufferSize);
        }

        /// <summary>
        /// Trả lại một bộ đệm vào pool.
        /// </summary>
        /// <param name="buffer">Bộ đệm để trả lại.</param>
        public void ReleaseBuffer(byte[] buffer)
        {
            if (buffer == null || buffer.Length != _bufferSize)
            {
                return;
            }

            _freeBuffers.Enqueue(buffer);
        }

        /// <summary>
        /// Tăng dung lượng pool bằng cách thêm các bộ đệm.
        /// </summary>
        /// <param name="additionalCapacity">Số lượng bộ đệm thêm vào.</param>
        public void IncreaseCapacity(int additionalCapacity)
        {
            if (additionalCapacity <= 0)
            {
                throw new ArgumentException("Số lượng bổ sung phải lớn hơn không.");
            }

            var buffersToAdd = new List<byte[]>(additionalCapacity);
            for (int i = 0; i < additionalCapacity; ++i)
            {
                buffersToAdd.Add(_arrayPool.Rent(_bufferSize));
            }

            foreach (var buffer in buffersToAdd)
            {
                _freeBuffers.Enqueue(buffer);
            }

            Interlocked.Add(ref _totalBuffers, additionalCapacity);
        }

        /// <summary>
        /// Giảm dung lượng pool bằng cách loại bỏ các bộ đệm.
        /// </summary>
        /// <param name="capacityToRemove">Số lượng bộ đệm để loại bỏ.</param>
        public void DecreaseCapacity(int capacityToRemove)
        {
            if (capacityToRemove <= 0)
            {
                return;
            }

            for (int i = 0; i < capacityToRemove; ++i)
            {
                if (_freeBuffers.TryDequeue(out var buffer))
                {
                    _arrayPool.Return(buffer);
                    Interlocked.Decrement(ref _totalBuffers);
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Lấy thông tin về pool bộ đệm.
        /// </summary>
        /// <param name="freeCount">Số lượng bộ đệm rảnh.</param>
        /// <param name="totalBuffers">Tổng số bộ đệm.</param>
        /// <param name="bufferSize">Kích thước của mỗi bộ đệm.</param>
        /// <param name="misses">Số lần thiếu bộ đệm.</param>
        public void GetInfo(out int freeCount, out int totalBuffers, out int bufferSize, out int misses)
        {
            freeCount = FreeBuffers;
            totalBuffers = TotalBuffers;
            bufferSize = _bufferSize;
            misses = _misses;
        }

        /// <summary>
        /// Giải phóng pool bộ đệm và trả lại tất cả các bộ đệm vào pool mảng.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Thực hiện giải phóng tài nguyên.
        /// </summary>
        /// <param name="disposing">Chỉ định liệu việc giải phóng có được gọi từ Dispose hay không.</param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            lock (_disposeLock)
            {
                if (_disposed)
                {
                    return;
                }

                if (disposing)
                {
                    // Giải phóng tài nguyên được quản lý
                    while (_freeBuffers.TryDequeue(out var buffer))
                    {
                        _arrayPool.Return(buffer);
                    }

                    GlobalPools.TryRemove(_bufferSize, out _);
                }

                // Nếu cần, thêm giải phóng tài nguyên không được quản lý ở đây

                _disposed = true;
            }
        }

        // Finalizer (chỉ sử dụng nếu cần giải phóng tài nguyên không được quản lý)
        ~SharedBufferPool()
        {
            Dispose(disposing: false);
        }
    }
}