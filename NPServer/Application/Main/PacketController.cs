using NPServer.Application.Handlers;
using NPServer.Application.Helper;
using NPServer.Core.Interfaces.Packets;
using NPServer.Core.Interfaces.Session;
using NPServer.Core.Memory;
using NPServer.Core.Packets;
using NPServer.Core.Packets.Queue;
using NPServer.Core.Packets.Utilities;
using NPServer.Infrastructure.Logging;
using NPServer.Infrastructure.Services;
using NPServer.Models.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Application.Main;

/// <summary>
/// Bộ điều khiển xử lý gói tin cho server.
/// </summary>
internal sealed class PacketController(CancellationToken token)
{
    private readonly ObjectPool _packetPool = new();
    private readonly CancellationToken _token = token;
    private readonly CommandDispatcher _commandDispatcher = new();
    private readonly PacketQueueManager _packetQueueManager = new();
    private readonly ISessionManager _sessionManager = Singleton.GetInstanceOfInterface<ISessionManager>();

    public void InitializePacketProcessingTasks()
    {
        Task.Run(() => RunQueueProcessor(PacketQueueType.Incoming, packet =>
            ProcessOutgoingPacket(packet, _packetQueueManager.GetQueue(PacketQueueType.Outgoing), _packetQueueManager.GetQueue(PacketQueueType.Server))
        ), _token);

        Task.Run(() => RunQueueProcessor(PacketQueueType.Outgoing, ProcessIncomingPacket), _token);
    }

    public void EnqueueIncomingPacket(UniqueId id, byte[] data)
    {
        if (!PacketValidation.ValidatePacketStructure(data)) return;

        Packet packet = _packetPool.Get<Packet>();

        packet.SetId(id);
        packet.ParseFromBytes(data);

        _packetQueueManager.GetQueue(PacketQueueType.Incoming).Enqueue(packet);
    }

    private void RunQueueProcessor(PacketQueueType queueType, Action<IPacket> processPacket)
    {
        try
        {
            while (!_token.IsCancellationRequested)
            {
                _packetQueueManager.WaitForQueue(queueType, _token);

                var packetsBatch = _packetQueueManager
                    .GetQueue(queueType)
                    .DequeueBatch(50);

                ProcessPacketBatch(queueType, packetsBatch, processPacket);
            }
        }
        catch (OperationCanceledException)
        {
            // Token đã bị hủy, kết thúc xử lý
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error processing queue ({queueType}): {ex.Message}");
        }
    }

    private void ProcessPacketBatch(PacketQueueType queueType, List<IPacket> packetsBatch, Action<IPacket> processPacket)
    {
        try
        {
            Parallel.ForEach(packetsBatch, packet =>
            {
                try
                {
                    processPacket(packet);
                    _packetPool.Return((Packet)packet);
                }
                catch (Exception ex)
                {
                    NPLog.Instance.Error<PacketController>($"Error processing packet in {queueType} queue: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error processing batch in {queueType} queue: {ex.Message}");
        }
    }

    private void ProcessOutgoingPacket(IPacket packet, PacketQueue outgoingQueue, PacketQueue inserverQueue)
    {
        try
        {
            if (!_sessionManager.TryGetSession(packet.Id, out var session) || session == null)
                return;

            (object packetToSend, object? packetFromServer) = _commandDispatcher.HandleCommand(
                new CommandInput(packet, (Command)packet.Cmd, session.Role));

            if (packetToSend is string)
            {
                NPLog.Instance.Info<PacketController>($"PacketToSend is a string: {packetToSend}");
            }
            else if (packetToSend is IPacket outPacket)
            {
                outgoingQueue.Enqueue(outPacket);
            }

            if (packetFromServer is IPacket inPacket)
            {
                inserverQueue.Enqueue(inPacket);
            }
        }
        catch (Exception ex)
        {
            NPLog.Instance.Error<PacketController>($"[ProcessOutgoingPacket] Error processing packet: {ex}");
        }
    }

    private void ProcessIncomingPacket(IPacket packet)
    {
        try
        {
            if (!_sessionManager.TryGetSession(packet.Id, out var session) || session == null)
                return;

            session.UpdateLastActivityTime();

            if (packet.PayloadData.Length == 0)
                return;

            RetryHelper.Execute(() => session.Network.Send(packet.ToByteArray()), maxRetries: 3, delayMs: 100,
                onRetry: attempt => NPLog.Instance.Warning<PacketController>($"Retrying send for packet {packet.Id}, attempt {attempt}."),
                onFailure: () => NPLog.Instance.Error<PacketController>($"Failed to send packet {packet.Id} after retries.")
            );
        }
        catch (Exception ex)
        {
            NPLog.Instance.Error<PacketController>($"[ProcessIncomingPacket] Error sending packet: {ex}");
        }
    }
}