using NPServer.Application.Handlers.Packets;
using NPServer.Application.Handlers.Packets.Queue;
using NPServer.Core.Interfaces.Packets;
using NPServer.Core.Interfaces.Pooling;
using NPServer.Core.Interfaces.Session;
using NPServer.Core.Packets.Utilities;
using NPServer.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Application.Main
{
    /// <summary>
    /// Lớp PacketContainer chịu trách nhiệm xử lý các gói tin đến và đi.
    /// </summary>
    internal class PacketController
    {
        private readonly IPacketPool _packetPool;
        private readonly CancellationToken _token;
        private readonly PacketIncoming _incomingPacket;
        private readonly ParallelOptions _parallelOptions;
        private readonly PacketProcessor _packetProcessor;
        private readonly PacketQueueManager _packetQueueManager;

        /// <summary>
        /// Khởi tạo một đối tượng <see cref="PacketController"/> mới.
        /// </summary>
        /// <param name="token">Token hủy bỏ cho các tác vụ bất đồng bộ.</param>
        public PacketController(CancellationToken token)
        {
            _token = token;
            _incomingPacket = Singleton.GetInstance<PacketIncoming>();

            _packetPool = Singleton.GetInstanceOfInterface<IPacketPool>();
            _packetProcessor = new PacketProcessor(Singleton.GetInstanceOfInterface<ISessionManager>());
            _packetQueueManager = new PacketQueueManager(
                Singleton.GetInstance<PacketInserver>(), _incomingPacket, Singleton.GetInstance<PacketOutgoing>()
            );

            _parallelOptions = new()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = _token
            };
        }

        public void EnqueueIncomingPacket(UniqueId id, byte[] data)
        {
            if (!PacketValidation.ValidatePacketStructure(data)) return;

            IPacket packet = _packetPool.RentPacket();

            packet.SetId(id);
            packet.ParseFromBytes(data);

            _incomingPacket.Enqueue(packet);
        }

        /// <summary>
        /// Xử lý các gói tin đến.
        /// </summary>
        public async Task ProcessIncomingPackets()
        {
            while (!_token.IsCancellationRequested)
            {
                await _packetQueueManager.WaitForIncoming(_token);
                List<IPacket> packetsBatch = _packetQueueManager.IncomingPacketQueue.DequeueBatch(50);

                await HandleIncomingPacketBatch(packetsBatch);
            }
        }

        /// <summary>
        /// Xử lý các gói tin đi.
        /// </summary>
        public async Task ProcessOutgoingPackets()
        {
            while (!_token.IsCancellationRequested)
            {
                await _packetQueueManager.WaitForOutgoing(_token);
                List<IPacket> packetsBatch = _packetQueueManager.OutgoingPacketQueue.DequeueBatch(50);

                await HandleOutgoingPacketBatch(packetsBatch);
            }
        }

        /// <summary>
        /// Xử lý một batch các gói tin đến.
        /// </summary>
        /// <param name="packetsBatch">Danh sách các gói tin cần xử lý.</param>
        private async Task HandleIncomingPacketBatch(List<IPacket> packetsBatch)
        {
            await Parallel.ForEachAsync(packetsBatch, _parallelOptions, async (packet, token) =>
            {
                await _packetProcessor.HandleIncomingPacket(packet, _packetQueueManager.OutgoingPacketQueue);
            });
        }

        /// <summary>
        /// Xử lý một batch các gói tin đi.
        /// </summary>
        /// <param name="packetsBatch">Danh sách các gói tin cần xử lý.</param>
        private async Task HandleOutgoingPacketBatch(List<IPacket> packetsBatch)
        {
            await Parallel.ForEachAsync(packetsBatch, _parallelOptions, async (packet, token) =>
            {
                await _packetProcessor.HandleOutgoingPacket(packet);
            });
        }
    }
}