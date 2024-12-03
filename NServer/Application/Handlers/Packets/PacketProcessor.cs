using NServer.Application.Handlers.Enums;
using NServer.Application.Handlers.Packets.Queue;
using NServer.Core.Interfaces.Packets;
using NServer.Core.Interfaces.Session;
using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NServer.Application.Handlers.Packets
{
    /// <summary>
    /// Lớp chịu trách nhiệm xử lý gói tin đến và đi.
    /// </summary>
    internal class PacketProcessor(ISessionManager sessionManager)
    {
        private readonly ISessionManager _sessionManager = sessionManager;

        private readonly HashSet<Command> _commandsWithoutLoginRequired =
        [
            Command.PONG, Command.PING, Command.NONE, Command.HEARTBEAT,
            Command.CLOSE, Command.GET_KEY, Command.REGISTER, Command.LOGIN
        ];

        private readonly CommandDispatcher _commandDispatcher = Singleton.GetInstance<CommandDispatcher>();

        /// <summary>
        /// Xử lý gói tin đến.
        /// </summary>
        public async Task HandleIncomingPacket(IPacket packet, PacketOutgoing outgoingQueue)
        {
            try
            {
                if (!_sessionManager.TryGetSession(packet.Id, out var session) || session == null)
                    return;

                if (!session.Authenticator && !_commandsWithoutLoginRequired.Contains((Command)packet.Cmd))
                {
                    outgoingQueue.Enqueue(PacketUtils.Response(Command.ERROR, "You must log in first."));
                    return;
                }

                var responsePacket = await _commandDispatcher.HandleCommand(packet).ConfigureAwait(false);
                outgoingQueue.Enqueue(responsePacket);
            }
            catch (Exception ex)
            {
                NLog.Instance.Error<PacketProcessor>($"[HandleIncomingPacket] Error processing packet: {ex}");
            }
        }

        /// <summary>
        /// Xử lý gói tin đi.
        /// </summary>
        public async Task HandleOutgoingPacket(IPacket packet)
        {
            if (!_sessionManager.TryGetSession(packet.Id, out var session) || session == null)
                return;

            try
            {
                session.UpdateLastActivityTime();

                if (packet.Payload.Length == 0)
                    return;

                await RetryAsync(() => session.Network.Send(packet.ToByteArray()), maxRetries: 3, delayMs: 100);
            }
            catch (Exception ex)
            {
                NLog.Instance.Error<PacketProcessor>($"[HandleOutgoingPacket] Error sending packet: {ex}");
            }
        }

        /// <summary>
        /// Thực hiện lại hành động không đồng bộ nhiều lần nếu thất bại.
        /// </summary>
        private static async Task RetryAsync(Func<bool> action, int maxRetries, int delayMs)
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
                    NLog.Instance.Warning($"[RetryAsync] Attempt {attempt + 1} failed: {ex.Message}");
                }

                await Task.Delay(delayMs).ConfigureAwait(false);
            }

            throw new InvalidOperationException("All retry attempts failed.");
        }
    }
}