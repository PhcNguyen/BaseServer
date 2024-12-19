using NPServer.Core.Interfaces.Session;
using NPServer.Infrastructure.Logging;
using NPServer.Core.Interfaces.Packets;
using NPServer.Models.Common;
using System.Threading;
using System;
using NPServer.Core.Packets.Queue;

namespace NPServer.Application.Handlers;

internal sealed class PacketProcessor(ISessionManager sessionManager)
{
    private readonly ISessionManager _sessionManager = sessionManager;
    private readonly CommandDispatcher _commandDispatcher = new();

    public void HandleIncomingPacket(IPacket packet, PacketQueue outgoingQueue, PacketQueue inserverQueue)
    {
        try
        {
            if (!_sessionManager.TryGetSession(packet.Id, out var session) || session == null)
                return;

            (object packetToSend, object? packetFromServer) = _commandDispatcher.HandleCommand(
                new CommandInput(packet, (Command)packet.Cmd, session.Role));

            if (packetToSend is string)
            {
                // Xử lý nếu packetToSend là string.
                // Có thể log thông báo hoặc xử lý gì đó khác.
                NPLog.Instance.Info<PacketProcessor>($"PacketToSend is a string: {packetToSend}");
            }
            else if (packetToSend is IPacket outPacket)
                outgoingQueue.Enqueue(outPacket);

            if (packetFromServer is IPacket inPacket)
                inserverQueue.Enqueue(inPacket);
        }
        catch (Exception ex)
        {
            NPLog.Instance.Error<PacketProcessor>($"[HandlePacketProcessing] Error processing packet: {ex}");
        }
    }

    public void HandleOutgoingPacket(IPacket packet)
    {
        try
        {
            if (!_sessionManager.TryGetSession(packet.Id, out var session) || session == null)
                return;

            session.UpdateLastActivityTime();

            if (packet.PayloadData.Length == 0)
                return;

            RetryAsync(() => session.Network.Send(packet.ToByteArray()), maxRetries: 3, delayMs: 100);
        }
        catch (Exception ex)
        {
            NPLog.Instance.Error<PacketProcessor>($"[ProcessIncomingPacket] Error sending packet: {ex}");
        }
    }

    private static void RetryAsync(Func<bool> action, int maxRetries, int delayMs)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                if (action())
                    return;
            }
            catch (Exception ex) when (attempt < maxRetries - 1)
            {
                NPLog.Instance.Warning<PacketProcessor>($"[RetryAsync] Attempt {attempt + 1} failed: {ex.Message}");
            }

            Thread.Sleep(delayMs);
        }

        throw new InvalidOperationException("All retry attempts failed.");
    }
}
