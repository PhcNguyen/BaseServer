using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace NPServer.Application.Handlers.Packets.Queue;

public class PacketQueueManager : IAsyncDisposable, IDisposable
{
    private readonly IReadOnlyDictionary<PacketQueueType, PacketQueue> _queues;
    private readonly IReadOnlyDictionary<PacketQueueType, SemaphoreSlim> _signals;

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
    /// <returns>Đối tượng PacketQueue.</returns>
    public PacketQueue GetQueue(PacketQueueType queueType) =>
        _queues.TryGetValue(queueType, out var queue) ? queue : throw new InvalidOperationException($"Queue type {queueType} not found.");

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

    private void ReleaseSignal(PacketQueueType queueType)
    {
        if (_signals.TryGetValue(queueType, out var signal))
        {
            signal.Release();
        }
    }

    public void Dispose()
    {
        foreach (var signal in _signals.Values)
        {
            signal?.Dispose();
        }

        GC.SuppressFinalize(this);
    }

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