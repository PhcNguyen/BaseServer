using NPServer.Application.Handlers.Packets.Queue;
using NPServer.Commands;
using NPServer.Core.Interfaces.Session;
using NPServer.Infrastructure.Logging;
using NPServer.Models.Common;
using System;
using System.Threading;

namespace NPServer.Application.Handlers.Packets;

/// <summary>
/// Lớp chịu trách nhiệm xử lý gói tin đến và đi.
/// </summary>
internal sealed class PacketProcessor(ISessionManager sessionManager)
{
    private readonly ISessionManager _sessionManager = sessionManager;
    private readonly CommandController _commandPacketDispatcher = new();

    /// <summary>
    /// Xử lý gói tin đến.
    /// </summary>
    public void HandleIncomingPacket(Packet packet, PacketQueue outgoingQueue, PacketQueue inserverQueue)
    {
        try
        {
            if (!_sessionManager.TryGetSession(packet.Id, out var session) || session == null)
                return;

            var (outgoingPacket, inServerPacket) = _commandPacketDispatcher.HandleCommand(
                new CommandInput(packet, (Command)packet.Cmd, session.Role));

            if (outgoingPacket is Packet validOutgoingPacket)
                outgoingQueue.Enqueue(validOutgoingPacket);

            if (inServerPacket is Packet validInServerPacket)
                inserverQueue.Enqueue(validInServerPacket);
        }
        catch (Exception ex)
        {
            NPLog.Instance.Error<PacketProcessor>($"[HandleIncomingPacket] Error processing packet: {ex}");
        }
    }

    /// <summary>
    /// Xử lý gói tin đi.
    /// </summary>
    public void HandleOutgoingPacket(Packet packet)
    {
        if (!_sessionManager.TryGetSession(packet.Id, out var session) || session == null)
            return;

        try
        {
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

    /// <summary>
    /// Thực hiện lại hành động không đồng bộ nhiều lần nếu thất bại.
    /// </summary>
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