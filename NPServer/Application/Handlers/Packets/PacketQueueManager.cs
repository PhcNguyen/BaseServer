using NPServer.Core.Interfaces.Communication;
using NPServer.Infrastructure.Collections;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Application.Handlers.Packets
{
    internal class PacketQueue : CustomQueues<IPacket>
    {
        public PacketQueue() : base()
        {
        }
    }

    /// <summary>
    /// Lớp PacketQueueManager chịu trách nhiệm quản lý hàng đợi các gói tin đến và đi.
    /// </summary>
    internal class PacketQueueManager : IDisposable
    {
        private readonly SemaphoreSlim _signal = new(0);
        private readonly PacketQueue _inserverPacketQueue = new();
        private readonly PacketQueue _incomingPacketQueue = new();
        private readonly PacketQueue _outgoingPacketQueue = new();

        /// <summary>
        /// Hàng đợi các gói tin trong server.
        /// </summary>
        public PacketQueue InserverPacketQueue => _inserverPacketQueue;

        /// <summary>
        /// Hàng đợi các gói tin đến.
        /// </summary>
        public PacketQueue IncomingPacketQueue => _incomingPacketQueue;

        /// <summary>
        /// Hàng đợi các gói tin đi.
        /// </summary>
        public PacketQueue OutgoingPacketQueue => _outgoingPacketQueue;

        public PacketQueueManager()
        {
            _inserverPacketQueue.PacketAdded += () => ReleaseSignal();
            _incomingPacketQueue.PacketAdded += () => ReleaseSignal();
            _outgoingPacketQueue.PacketAdded += () => ReleaseSignal();
        }

        /// <summary>
        /// Chờ tín hiệu cho gói tin đến.
        /// </summary>
        public async Task WaitForIncoming(CancellationToken cancellationToken)
        {
            await WaitForSignal(cancellationToken);
        }

        /// <summary>
        /// Chờ tín hiệu cho gói tin đi.
        /// </summary>
        public async Task WaitForOutgoing(CancellationToken cancellationToken)
        {
            await WaitForSignal(cancellationToken);
        }

        /// <summary>
        /// Chờ tín hiệu cho gói tin trong server.
        /// </summary>
        public async Task WaitForInserver(CancellationToken cancellationToken)
        {
            await WaitForSignal(cancellationToken);
        }

        private async Task WaitForSignal(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
        }

        private void ReleaseSignal()
        {
            try
            {
                _signal.Release();
            }
            catch (SemaphoreFullException)
            {
                // Ignore if already released
            }
        }

        public void Dispose()
        {
            _signal.Dispose();
        }
    }
}