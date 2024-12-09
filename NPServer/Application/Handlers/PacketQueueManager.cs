using NPServer.Core.Interfaces.Communication;
using NPServer.Infrastructure.Collections;
using System;
using System.Threading;
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
        INSERVER,
        INCOMING,
        OUTGOING
    }

    /// <summary>
    /// Lớp PacketQueueManager chịu trách nhiệm quản lý hàng đợi các gói tin đến và đi.
    /// </summary>
    public class PacketQueueManager : IDisposable
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
                _queues[type] = queue;
                _signals[type] = signal;
            }
        }

        public PacketQueue GetQueue(PacketQueueType queueType) =>
            _queues.TryGetValue(queueType, out PacketQueue? queue)
                ? queue : throw new InvalidOperationException($"Queue type {queueType} not found.");

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

        private void ReleaseSignal(PacketQueueType queueType)
        {
            if (_signals.TryGetValue(queueType, out var signal))
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
        }

        public void Dispose()
        {
            foreach (var signal in _signals.Values)
            {
                signal?.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
