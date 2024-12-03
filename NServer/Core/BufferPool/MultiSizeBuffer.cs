using System;
using System.Linq;

namespace NServer.Core.BufferPool
{
    public class MultiSizeBuffer
    {
        private readonly (int BufferSize, double Allocation)[] _bufferAllocations;
        private readonly int _totalBuffers;
        private bool _isInitialized;

        private readonly BufferPoolManager _poolManager = new();
       
        public MultiSizeBuffer((int BufferSize, double Allocation)[] bufferAllocations, int totalBuffers)
        {
            _bufferAllocations = bufferAllocations;
            _totalBuffers = totalBuffers;

            _poolManager.EventShrink += ShrinkBufferPoolSize;
            _poolManager.EventIncrease += IncreaseBufferPoolSize;
        }

        public void AllocateBuffers()
        {
            if (_isInitialized) throw new InvalidOperationException("Buffers already allocated.");

            foreach (var (bufferSize, allocation) in _bufferAllocations)
            {
                int capacity = (int)(_totalBuffers * allocation);
                _poolManager.CreatePool(bufferSize, capacity);
            }
            _isInitialized = true;
        }

        public byte[] RentBuffer(int size = 256) => _poolManager.RentBuffer(size);

        public void ReturnBuffer(byte[] buffer) => _poolManager.ReturnBuffer(buffer);

        private void ShrinkBufferPoolSize(SharedBufferPool pool)
        {
            pool.GetInfo(out int free, out int total, out int size, out _);

            int buffersToShrink = Math.Max(0, (int)(free - GetAllocationForSize(size) * _totalBuffers) - (int)(total * 0.2));
            buffersToShrink = Math.Min(buffersToShrink, 20);

            if (buffersToShrink > 0)
            {
                pool.DecreaseCapacity(buffersToShrink);
            }
        }

        private void IncreaseBufferPoolSize(SharedBufferPool pool)
        {
            if (pool.FreeBuffers <= pool.TotalBuffers * 0.2)
            {
                int increaseBy = Math.Max(1, pool.TotalBuffers / 4);
                pool.IncreaseCapacity(increaseBy);
            }
        }

        public double GetAllocationForSize(int size)
        {
            foreach (var (bufferSize, allocation) in _bufferAllocations.OrderBy(alloc => alloc.BufferSize))
            {
                if (bufferSize >= size)
                    return allocation;
            }
            throw new ArgumentException($"No allocation found for size: {size}");
        }

        public void Dispose() => _poolManager.Dispose();
    }
}