using NPServer.Application.Handlers.Packets.Queue;
using NPServer.Application.Handlers.Packets;
using NPServer.Application.Handlers;
using NPServer.Commands;
using NPServer.Core.Interfaces.Session;
using NPServer.Infrastructure.Logging;
using NPServer.Models.Common;
using System.Threading;
using System;

internal sealed class PacketProcessor(ISessionManager sessionManager)
{
    private readonly ISessionManager _sessionManager = sessionManager;
    private readonly CommandDispatcher _commandPacketDispatcher = new();

    public void HandleIncomingPacket(Packet packet, PacketQueue outgoingQueue, PacketQueue inserverQueue)
    {
        try
        {
            if (!_sessionManager.TryGetSession(packet.Id, out var session) || session == null)
                return;

            (object packetToSend, object? packetFromServer) = _commandPacketDispatcher.HandleCommand(
                new CommandInput(packet, (Command)packet.Cmd, session.Role));

            if (packetToSend is string)
            {
                // Xử lý nếu packetToSend là string.
                // Có thể log thông báo hoặc xử lý gì đó khác.
                NPLog.Instance.Info<PacketProcessor>($"PacketToSend is a string: {packetToSend}");
            }
            else if (packetToSend is Packet outPacket)
                outgoingQueue.Enqueue(outPacket);

            if (packetFromServer is Packet inPacket)
                inserverQueue.Enqueue(inPacket);
        }
        catch (Exception ex)
        {
            NPLog.Instance.Error<PacketProcessor>($"[HandleIncomingPacket] Error processing packet: {ex}");
        }
    }

    public void HandleOutgoingPacket(Packet packet)
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
            NPLog.Instance.Error<PacketProcessor>($"[HandleOutgoingPacket] Error sending packet: {ex}");
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
