using NPServer.Application.Handlers;
using NPServer.Application.Handlers.Packets.Queue;
using NPServer.Core.Interfaces.Packets;
using NPServer.Core.Interfaces.Pooling;
using NPServer.Core.Interfaces.Session;
using NPServer.Core.Packets.Utilities;
using NPServer.Infrastructure.Logging;
using NPServer.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace NPServer.Application.Handlers.Packets
{
    /// <summary>
    /// Lớp chịu trách nhiệm xử lý gói tin đến và đi.
    /// </summary>
    internal class PacketProcessor(ISessionManager sessionManager)
    {
        private readonly IPacketPool _packetPool = Singleton.GetInstanceOfInterface<IPacketPool>();
        private readonly ISessionManager _sessionManager = sessionManager;

        private readonly Command[] _commandsWithoutLoginRequired =
        [
            Command.PONG, Command.PING, Command.NONE, Command.HEARTBEAT,
            Command.CLOSE, Command.GET_KEY, Command.REGISTER, Command.LOGIN
        ];

        private readonly CommandDispatcher _commandDispatcher = Singleton.GetInstanceOfInterface<CommandDispatcher>();

        /// <summary>
        /// Xử lý gói tin đến.
        /// </summary>
        public async Task HandleIncomingPacket(IPacket packet, PacketOutgoing outgoingQueue)
        {
            try
            {
                if (!_sessionManager.TryGetSession(packet.Id, out var session) || session == null)
                    return;

                if (!session.Authenticator && IsLoginRequired((Command)packet.Cmd))
                {
                    outgoingQueue.Enqueue(((short)Command.ERROR).ToResponsePacket("You must log in first."));
                    return;
                }

                IPacket responsePacket = await _commandDispatcher.HandleCommand(packet).ConfigureAwait(false);

                _packetPool.ReturnPacket(packet);
                outgoingQueue.Enqueue(responsePacket);
            }
            catch (Exception ex)
            {
                NPLog.Instance.Error<PacketProcessor>($"[HandleIncomingPacket] Error processing packet: {ex}");
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

                _packetPool.ReturnPacket(packet);
            }
            catch (Exception ex)
            {
                NPLog.Instance.Error<PacketProcessor>($"[HandleOutgoingPacket] Error sending packet: {ex}");
            }
        }

        private bool IsLoginRequired(Command cmd)
        {
            Span<Command> commandsSpan = _commandsWithoutLoginRequired;
            foreach (var command in commandsSpan)
            {
                if (command == cmd) return false;
            }
            return true;
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
                    NPLog.Instance.Warning<PacketProcessor>($"[RetryAsync] Attempt {attempt + 1} failed: {ex.Message}");
                }

                await Task.Delay(delayMs).ConfigureAwait(false);
            }

            throw new InvalidOperationException("All retry attempts failed.");
        }
    }
}