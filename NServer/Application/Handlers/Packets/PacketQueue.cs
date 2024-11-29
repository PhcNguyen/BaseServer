using System.Threading;
using System.Threading.Tasks;
using NServer.Core.Interfaces.Packets;

namespace NServer.Application.Handlers.Packets
{
    /// <summary>
    /// Lớp PacketQueue chịu trách nhiệm quản lý hàng đợi các gói tin đến và đi.
    /// </summary>
    internal class PacketQueue
    {
        private readonly SemaphoreSlim _incomingSignal = new(0);
        private readonly SemaphoreSlim _outgoingSignal = new(0);

        /// <summary>
        /// Hàng đợi các gói tin đến.
        /// </summary>
        public IPacketIncoming IncomingPacketQueue { get; }

        /// <summary>
        /// Hàng đợi các gói tin đi.
        /// </summary>
        public IPacketOutgoing OutgoingPacketQueue { get; }

        /// <summary>
        /// Khởi tạo một đối tượng <see cref="PacketQueue"/> mới.
        /// </summary>
        /// <param name="incomingQueue">Hàng đợi các gói tin đến.</param>
        /// <param name="outgoingQueue">Hàng đợi các gói tin đi.</param>
        public PacketQueue(IPacketIncoming incomingQueue, IPacketOutgoing outgoingQueue)
        {
            IncomingPacketQueue = incomingQueue;
            OutgoingPacketQueue = outgoingQueue;

            IncomingPacketQueue.PacketAdded += () => _incomingSignal.Release();
            OutgoingPacketQueue.PacketAdded += () => _outgoingSignal.Release();
        }

        /// <summary>
        /// Chờ tín hiệu cho gói tin đến.
        /// </summary>
        /// <param name="cancellationToken">Token hủy bỏ cho tác vụ bất đồng bộ.</param>
        public async Task WaitForIncomingSignal(CancellationToken cancellationToken)
        {
            await _incomingSignal.WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Chờ tín hiệu cho gói tin đi.
        /// </summary>
        /// <param name="cancellationToken">Token hủy bỏ cho tác vụ bất đồng bộ.</param>
        public async Task WaitForOutgoingSignal(CancellationToken cancellationToken)
        {
            await _outgoingSignal.WaitAsync(cancellationToken);
        }
    }
}