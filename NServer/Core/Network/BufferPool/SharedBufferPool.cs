using System;
using System.Buffers;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NServer.Core.Network.BufferPool
{
    /// <summary>
    /// Quản lý một pool của các bộ đệm dùng chung.
    /// </summary>
    public class SharedBufferPool
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
            return GlobalPools.GetOrAdd(bufferSize, _ => new SharedBufferPool(bufferSize, initialCapacity));
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
            if (!_disposed)
            {
                lock (_disposeLock)
                {
                    if (!_disposed)
                    {
                        _disposed = true;

                        while (_freeBuffers.TryDequeue(out var buffer))
                        {
                            _arrayPool.Return(buffer);
                        }

                        GlobalPools.TryRemove(_bufferSize, out _);
                    }
                }
            }
        }
    }
}