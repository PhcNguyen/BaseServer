using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Core.Packets.Queue;

/// <summary>
/// Quản lý các hàng đợi gói tin và cung cấp cơ chế chờ cho các gói tin được thêm vào.
/// </summary>
public class PacketQueueManager : IAsyncDisposable, IDisposable
{
    private readonly Dictionary<PacketQueueType, PacketQueue> _queues;
    private readonly IReadOnlyDictionary<PacketQueueType, SemaphoreSlim> _signals;

    /// <summary>
    /// Khởi tạo một instance của <see cref="PacketQueueManager"/> class.
    /// </summary>
    public PacketQueueManager()
    {
        Dictionary<PacketQueueType, PacketQueue> queues = [];
        Dictionary<PacketQueueType, SemaphoreSlim> signals = [];

        foreach (PacketQueueType type in Enum.GetValues<PacketQueueType>())
        {
            var queue = new PacketQueue();
            var signal = new SemaphoreSlim(0);

            queue.PacketAdded += () => ReleaseSignal(type);

            queues[type] = queue;
            signals[type] = signal;
        }

        _queues = queues;
        _signals = signals;
    }

    /// <summary>
    /// Lấy hàng đợi tương ứng với loại gói tin.
    /// </summary>
    /// <param name="queueType">Loại hàng đợi.</param>
    /// <returns>Đối tượng <see cref="PacketQueue"/> tương ứng với loại gói tin đã chỉ định.</returns>
    public PacketQueue GetQueue(PacketQueueType queueType) =>
        _queues.TryGetValue(queueType, out var queue) ? queue : throw new InvalidOperationException($"Queue type {queueType} not found.");

    /// <summary>
    /// Chờ cho hàng đợi có gói tin mới được thêm vào.
    /// </summary>
    /// <param name="queueType">Loại hàng đợi cần chờ.</param>
    /// <param name="cancellationToken">Token để hủy thao tác chờ khi cần thiết.</param>
    public void WaitForQueue(PacketQueueType queueType, CancellationToken cancellationToken)
    {
        if (_signals.TryGetValue(queueType, out var signal))
        {
            signal.Wait(cancellationToken);
        }
        else
        {
            throw new InvalidOperationException($"Signal for queue type {queueType} not found.");
        }
    }

    /// <summary>
    /// Chờ hàng đợi có gói tin mới một cách bất đồng bộ.
    /// </summary>
    /// <param name="queueType">Loại hàng đợi cần chờ.</param>
    /// <param name="cancellationToken">Token để hủy thao tác chờ khi cần thiết.</param>
    public async Task WaitForQueueAsync(PacketQueueType queueType, CancellationToken cancellationToken)
    {
        if (_signals.TryGetValue(queueType, out var signal))
        {
            await signal.WaitAsync(cancellationToken);
        }
        else
        {
            throw new InvalidOperationException($"Signal for queue type {queueType} not found.");
        }
    }

    /// <summary>
    /// Giải phóng tín hiệu semaphore liên kết với loại hàng đợi được chỉ định.
    /// </summary>
    /// <param name="queueType">Loại hàng đợi cần giải phóng tín hiệu.</param>
    private void ReleaseSignal(PacketQueueType queueType)
    {
        if (_signals.TryGetValue(queueType, out var signal))
        {
            signal.Release();
        }
    }

    /// <summary>
    /// Giải phóng tài nguyên sử dụng bởi <see cref="PacketQueueManager"/>.
    /// </summary>
    public void Dispose()
    {
        foreach (var signal in _signals.Values)
        {
            signal?.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Giải phóng tài nguyên sử dụng bởi <see cref="PacketQueueManager"/> một cách bất đồng bộ.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        foreach (var signal in _signals.Values)
        {
            signal?.Dispose();
        }

        await Task.CompletedTask;
        GC.SuppressFinalize(this);
    }
}
