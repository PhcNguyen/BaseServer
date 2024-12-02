using NServer.Application.Handlers.Packets.Queue;
using System.Threading;
using System.Threading.Tasks;

namespace NServer.Application.Handlers.Packets
{
    /// <summary>
    /// Lớp PacketQueue chịu trách nhiệm quản lý hàng đợi các gói tin đến và đi.
    /// </summary>
    internal class PacketQueueManager
    {
        private readonly SemaphoreSlim _inserverSignal = new(0);
        private readonly SemaphoreSlim _incomingSignal = new(0);
        private readonly SemaphoreSlim _outgoingSignal = new(0);

        /// <summary>
        /// Hàng đợi các gói tin trong server.
        /// </summary>
        public PacketInserver InserverPacketQueue { get; }

        /// <summary>
        /// Hàng đợi các gói tin đến.
        /// </summary>
        public PacketIncoming IncomingPacketQueue { get; }

        /// <summary>
        /// Hàng đợi các gói tin đi.
        /// </summary>
        public PacketOutgoing OutgoingPacketQueue { get; }

        /// <summary>
        /// Khởi tạo một đối tượng <see cref="PacketQueueManager"/> mới.
        /// </summary>
        /// <param name="incomingQueue">Hàng đợi các gói tin đến.</param>
        /// <param name="outgoingQueue">Hàng đợi các gói tin đi.</param>
        public PacketQueueManager(PacketInserver inserverQueue, PacketIncoming incomingQueue, PacketOutgoing outgoingQueue)
        {
            InserverPacketQueue = inserverQueue;
            IncomingPacketQueue = incomingQueue;
            OutgoingPacketQueue = outgoingQueue;

            InserverPacketQueue.PacketAdded += () => _inserverSignal.Release();
            IncomingPacketQueue.PacketAdded += () => _incomingSignal.Release();
            OutgoingPacketQueue.PacketAdded += () => _outgoingSignal.Release();
        }

        /// <summary>
        /// Chờ tín hiệu cho gói tin đến.
        /// </summary>
        /// <param name="cancellationToken">Token hủy bỏ cho tác vụ bất đồng bộ.</param>
        public async Task WaitForIncoming(CancellationToken cancellationToken)
        {
            await _incomingSignal.WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Chờ tín hiệu cho gói tin đi.
        /// </summary>
        /// <param name="cancellationToken">Token hủy bỏ cho tác vụ bất đồng bộ.</param>
        public async Task WaitForOutgoing(CancellationToken cancellationToken)
        {
            await _outgoingSignal.WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Chờ tín hiệu cho gói tin trong server.
        /// </summary>
        /// <param name="cancellationToken">Token hủy bỏ cho tác vụ bất đồng bộ.</param>
        public async Task WaitForInserver(CancellationToken cancellationToken)
        {
            await _outgoingSignal.WaitAsync(cancellationToken);
        }
    }
}