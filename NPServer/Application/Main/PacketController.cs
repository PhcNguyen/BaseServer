using NPServer.Core.Communication.Utilities;
using NPServer.Core.Interfaces.Session;
using NPServer.Core.Memory;
using NPServer.Infrastructure.Services;
using NPServer.Packets;
using NPServer.Packets.Queue;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Application.Main
{
    internal class PacketController
    {
        private readonly ObjectPool _packetPool;
        private readonly CancellationToken _token;
        private readonly ISessionManager _sessionManager;
        private readonly PacketProcessor _packetProcessor;
        private readonly PacketQueueManager _packetQueueManager;

        public PacketController(CancellationToken token)
        {
            _token = token;
            _packetPool = new ObjectPool();
            _packetQueueManager = new PacketQueueManager();
            _sessionManager = Singleton.GetInstanceOfInterface<ISessionManager>();
            _packetProcessor = new PacketProcessor(_sessionManager);
        }

        public void StartTasks()
        {
            Task.Run(() => StartProcessing(PacketQueueType.In, HandleIncomingPacketBatch), _token);
            Task.Run(() => StartProcessing(PacketQueueType.Out, HandleOutgoingPacketBatch), _token);
        }

        public void EnqueueIncomingPacket(UniqueId id, byte[] data)
        {
            if (!PacketValidation.ValidatePacketStructure(data)) return;

            Packet packet = _packetPool.Get<Packet>();

            packet.SetId(id);
            packet.ParseFromBytes(data);

            _packetQueueManager.GetQueue(PacketQueueType.In).Enqueue(packet);
        }

        private void StartProcessing(PacketQueueType queueType, Action<List<Packet>> processBatch)
        {
            try
            {
                while (!_token.IsCancellationRequested)
                {
                    // Chờ tín hiệu hàng đợi
                    _packetQueueManager.WaitForQueue(queueType, _token);

                    // Lấy batch gói tin từ hàng đợi
                    var packetsBatch = _packetQueueManager
                        .GetQueue(queueType)
                        .DequeueBatch(50);

                    // Xử lý batch gói tin
                    processBatch(packetsBatch);
                }
            }
            catch (OperationCanceledException)
            {
                // Token đã bị hủy, kết thúc xử lý
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in queue processing ({queueType}): {ex.Message}");
            }
        }

        private void HandleIncomingPacketBatch(List<Packet> packetsBatch)
        {
            Parallel.ForEach(packetsBatch, packet =>
            {
                try
                {
                    _packetProcessor.HandleIncomingPacket(packet,
                        _packetQueueManager.GetQueue(PacketQueueType.In),
                        _packetQueueManager.GetQueue(PacketQueueType.Server));
                    _packetPool.Return(packet);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing incoming packet: {ex.Message}");
                }
            });
        }

        private void HandleOutgoingPacketBatch(List<Packet> packetsBatch)
        {
            Parallel.ForEach(packetsBatch, packet =>
            {
                try
                {
                    _packetProcessor.HandleOutgoingPacket(packet);
                    _packetPool.Return(packet);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing outgoing packet: {ex.Message}");
                }
            });
        }
    }
}