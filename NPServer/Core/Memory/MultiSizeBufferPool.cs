using NPServer.Core.Buffers;
using NPServer.Core.Interfaces.Memory;
using System;
using System.Linq;

namespace NPServer.Core.Memory;

/// <summary>
/// Quản lý các bộ đệm có nhiều kích thước khác nhau.
/// </summary>
public sealed class MultiSizeBufferPool : IMultiSizeBufferPool
{
    private readonly (int BufferSize, double Allocation)[] _bufferAllocations;
    private readonly int _totalBuffers;
    private bool _isInitialized;

    private readonly BufferPoolManager _poolManager = new();

    /// <summary>
    /// Khởi tạo một thể hiện mới của lớp <see cref="MultiSizeBufferPool"/> với các cấu hình phân bổ bộ đệm và tổng số bộ đệm.
    /// </summary>
    /// <param name="bufferAllocations">Mảng các cấu hình phân bổ bộ đệm.</param>
    /// <param name="totalBuffers">Tổng số bộ đệm.</param>
    public MultiSizeBufferPool((int BufferSize, double Allocation)[] bufferAllocations, int totalBuffers)
    {
        _bufferAllocations = bufferAllocations;
        _totalBuffers = totalBuffers;

        _poolManager.EventShrink += ShrinkBufferPoolSize;
        _poolManager.EventIncrease += IncreaseBufferPoolSize;
    }

    /// <summary>
    /// Cấp phát các bộ đệm dựa trên cấu hình.
    /// </summary>
    /// <exception cref="InvalidOperationException">Ném ra nếu bộ đệm đã được cấp phát.</exception>
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

    /// <summary>
    /// Thuê một bộ đệm có ít nhất kích thước yêu cầu.
    /// </summary>
    /// <param name="size">Kích thước của bộ đệm cần thuê, mặc định là 256.</param>
    /// <returns>Một mảng byte của bộ đệm.</returns>
    public byte[] RentBuffer(int size = 256) => _poolManager.RentBuffer(size);

    /// <summary>
    /// Trả lại bộ đệm về bộ đệm thích hợp.
    /// </summary>
    /// <param name="buffer">Bộ đệm để trả lại.</param>
    public void ReturnBuffer(byte[] buffer) => _poolManager.ReturnBuffer(buffer);

    /// <summary>
    /// Lấy tỷ lệ phân bổ cho kích thước bộ đệm cho trước.
    /// </summary>
    /// <param name="size">Kích thước của bộ đệm.</param>
    /// <returns>Tỷ lệ phân bổ của kích thước bộ đệm.</returns>
    /// <exception cref="ArgumentException">Ném ra nếu không tìm thấy phân bổ cho kích thước bộ đệm.</exception>
    public double GetAllocationForSize(int size)
    {
        foreach (var (bufferSize, allocation) in _bufferAllocations.OrderBy(alloc => alloc.BufferSize))
        {
            if (bufferSize >= size)
                return allocation;
        }

        throw new ArgumentException($"No allocation found for size: {size}");
    }

    /// <summary>
    /// Giảm dung lượng bộ đệm nếu có nhiều bộ đệm rảnh.
    /// </summary>
    /// <param name="pool">Bộ đệm cần giảm dung lượng.</param>
    private void ShrinkBufferPoolSize(BufferPoolShared pool)
    {
        pool.GetInfo(out int free, out int total, out int size, out _);

        int buffersToShrink = Math.Max(0, (int)(free - GetAllocationForSize(size) * _totalBuffers) - (int)(total * 0.2));
        buffersToShrink = Math.Min(buffersToShrink, 20);

        if (buffersToShrink > 0)
        {
            pool.DecreaseCapacity(buffersToShrink);
        }
    }

    /// <summary>
    /// Tăng dung lượng bộ đệm nếu số lượng bộ đệm rảnh dưới ngưỡng cho phép.
    /// </summary>
    /// <param name="pool">Bộ đệm cần tăng dung lượng.</param>
    private void IncreaseBufferPoolSize(BufferPoolShared pool)
    {
        if (pool.FreeBuffers <= pool.TotalBuffers * 0.2)
        {
            int increaseBy = Math.Max(1, pool.TotalBuffers / 4);
            pool.IncreaseCapacity(increaseBy);
        }
    }

    /// <summary>
    /// Giải phóng tất cả các tài nguyên của các pool bộ đệm.
    /// </summary>
    public void Dispose() => _poolManager.Dispose();
}