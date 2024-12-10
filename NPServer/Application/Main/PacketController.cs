using NPServer.Application.Handlers;
using NPServer.Core.Communication.Utilities;
using NPServer.Core.Interfaces.Communication;
using NPServer.Core.Interfaces.Pooling;
using NPServer.Core.Interfaces.Session;
using NPServer.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Application.Main
{
    internal class PacketController
    {
        private readonly IPacketPool _packetPool;
        private readonly CancellationToken _token;
        private readonly ISessionManager _sessionManager;
        private readonly PacketProcessor _packetProcessor;
        private readonly PacketQueueManager _packetQueueManager;

        public PacketController(CancellationToken token)
        {
            _token = token;
            _packetQueueManager = new PacketQueueManager();
            _packetPool = Singleton.GetInstanceOfInterface<IPacketPool>();
            _sessionManager = Singleton.GetInstanceOfInterface<ISessionManager>();
            _packetProcessor = new PacketProcessor(_sessionManager, _packetPool);
        }

        public void StartAllTasks()
        {
            Task.Run(() => StartProcessing(PacketQueueType.INCOMING, HandleIncomingPacketBatch), _token);
            Task.Run(() => StartProcessing(PacketQueueType.OUTGOING, HandleOutgoingPacketBatch), _token);
        }

        private void StartTask(Func<Task> taskFunc)
        {
            Task.Run(async () =>
            {
                try
                {
                    await taskFunc();
                }
                catch (OperationCanceledException)
                {
                    // Token bị hủy, kết thúc Task
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in Task: {ex.Message}");
                }
            }, _token);
        }

        public void EnqueueIncomingPacket(UniqueId id, byte[] data)
        {
            if (!PacketValidation.ValidatePacketStructure(data)) return;

            var packet = _packetPool.RentPacket();
            packet.SetId(id);
            packet.ParseFromBytes(data);

            _packetQueueManager.GetQueue(PacketQueueType.INCOMING).Enqueue(packet);
        }

        private void StartProcessing(PacketQueueType queueType, Action<List<IPacket>> processBatch)
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

        private void HandleIncomingPacketBatch(List<IPacket> packetsBatch)
        {
            Parallel.ForEach(packetsBatch, packet =>
            {
                try
                {
                    _packetProcessor.HandleIncomingPacket(packet, _packetQueueManager.GetQueue(PacketQueueType.OUTGOING));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing incoming packet: {ex.Message}");
                }
            });
        }

        private void HandleOutgoingPacketBatch(List<IPacket> packetsBatch)
        {
            Parallel.ForEach(packetsBatch, packet =>
            {
                try
                {
                    _packetProcessor.HandleOutgoingPacket(packet);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing outgoing packet: {ex.Message}");
                }
            });
        }
    }
}
