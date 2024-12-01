using NServer.Application.Handlers.Packets;
using NServer.Core.Interfaces.Packets;
using NServer.Core.Interfaces.Session;
using NServer.Core.Packets.Queue;
using NServer.Core.Packets.Utils;
using NServer.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NServer.Application.Main
{
    /// <summary>
    /// Lớp PacketContainer chịu trách nhiệm xử lý các gói tin đến và đi.
    /// </summary>
    internal class PacketContainer
    {
        private readonly CancellationToken _token;
        private readonly Handlers.Packets.PacketQueue _packetQueue;
        private readonly PacketProcessor _packetHandler;
        private readonly PacketOutgoing _outgoingPacket;
        private readonly PacketIncoming _incomingPacket;
        private readonly ISessionManager _sessionManager;
        private readonly ParallelOptions _parallelOptions;

        /// <summary>
        /// Khởi tạo một đối tượng <see cref="PacketContainer"/> mới.
        /// </summary>
        /// <param name="token">Token hủy bỏ cho các tác vụ bất đồng bộ.</param>
        public PacketContainer(CancellationToken token)
        {
            _token = token;
            _outgoingPacket = Singleton.GetInstance<PacketOutgoing>();
            _incomingPacket = Singleton.GetInstance<PacketIncoming>();
            _sessionManager = Singleton.GetInstanceOfInterface<ISessionManager>();

            _packetHandler = new PacketProcessor(_sessionManager);
            _packetQueue = new Handlers.Packets.PacketQueue(_incomingPacket, _outgoingPacket);

            _parallelOptions = new()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = _token
            };
        }

        public void EnqueueIncomingPacket(UniqueId id, byte[] data)
        {
            if (PacketValidation.IsValidPacket(data)) return;
            IPacket packet = PacketExtensions.FromByteArray(data);
            packet.SetId(id);

            _incomingPacket.Enqueue(packet);
        }

        /// <summary>
        /// Xử lý các gói tin đến.
        /// </summary>
        public async Task ProcessIncomingPackets()
        {
            while (!_token.IsCancellationRequested)
            {
                await _packetQueue.WaitForIncomingSignal(_token);
                List<IPacket> packetsBatch = _packetQueue.IncomingPacketQueue.DequeueBatch(50);

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
                await _packetQueue.WaitForOutgoingSignal(_token);
                List<IPacket> packetsBatch = _packetQueue.OutgoingPacketQueue.DequeueBatch(50);

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
                await _packetHandler.HandleIncomingPacket(packet, _packetQueue.OutgoingPacketQueue);
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
                await _packetHandler.HandleOutgoingPacket(packet);
            });
        }
    }
}