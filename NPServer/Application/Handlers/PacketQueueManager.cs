using NPServer.Core.Interfaces.Communication;
using NPServer.Infrastructure.Collections;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NPServer.Application.Handlers
{
    public class PacketQueue : CustomQueues<IPacket>
    {
        public PacketQueue() : base()
        {
        }
    }

    public enum PacketQueueType
    {
        Server,
        In,
        Out
    }

    /// <summary>
    /// Lớp PacketQueueManager chịu trách nhiệm quản lý hàng đợi các gói tin đến và đi.
    /// </summary>
    public class PacketQueueManager : IAsyncDisposable, IDisposable
    {
        private readonly Dictionary<PacketQueueType, PacketQueue> _queues;
        private readonly Dictionary<PacketQueueType, SemaphoreSlim> _signals;

        public PacketQueueManager()
        {
            _queues = [];
            _signals = [];

            foreach (PacketQueueType type in Enum.GetValues<PacketQueueType>())
            {
                var queue = new PacketQueue();
                var signal = new SemaphoreSlim(0);

                queue.PacketAdded += () => ReleaseSignal(type);
                queue.PacketAdded += async () => await ReleaseSignalAsync(type);

                _queues.TryAdd(type, queue);
                _signals.TryAdd(type, signal);
            }
        }

        /// <summary>
        /// Lấy hàng đợi tương ứng với loại gói tin.
        /// </summary>
        /// <param name="queueType">Loại hàng đợi.</param>
        /// <returns>Đối tượng PacketQueue.</returns>
        public PacketQueue GetQueue(PacketQueueType queueType) =>
            _queues.TryGetValue(queueType, out var queue)
            ? queue
            : throw new InvalidOperationException($"Queue type {queueType} not found.");

        public void WaitForQueue(PacketQueueType queueType, CancellationToken cancellationToken)
        {
            if (_signals.TryGetValue(queueType, out SemaphoreSlim? signal))
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
            if (_signals.TryGetValue(queueType, out SemaphoreSlim? signal))
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
            if (_signals.TryGetValue(queueType, out var signal) && signal.CurrentCount == 0)
            {
                signal.Release();
            }
        }

        private async Task ReleaseSignalAsync(PacketQueueType queueType)
        {
            if (_signals.TryGetValue(queueType, out var signal) && signal.CurrentCount == 0)
            {
                try
                {
                    signal.Release();
                }
                catch (SemaphoreFullException)
                {
                    // Ignore if already released
                }
            }

            await Task.CompletedTask;
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
}
