using NPServer.Core.Interfaces.Communication;
using NPServer.Core.Interfaces.Pooling;
using NPServer.Core.Interfaces.Session;
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

        /// <summary>
        /// Xử lý gói tin đến.
        /// </summary>
        public async Task HandleIncomingPacket(IPacket packet, PacketQueue outgoingQueue)
        {
            try
            {
                //IPacket responsePacket = await _commandDispatcher.HandleCommand(packet).ConfigureAwait(false);

                _packetPool.ReturnPacket(packet);
                //outgoingQueue.Enqueue(responsePacket);
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

                if (packet.PayloadData.Length == 0)
                    return;

                await RetryAsync(() => session.Network.Send(packet.ToByteArray()), maxRetries: 3, delayMs: 100);

                _packetPool.ReturnPacket(packet);
            }
            catch (Exception ex)
            {
                NPLog.Instance.Error<PacketProcessor>($"[HandleOutgoingPacket] Error sending packet: {ex}");
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
                    NPLog.Instance.Warning<PacketProcessor>($"[RetryAsync] Attempt {attempt + 1} failed: {ex.Message}");
                }

                await Task.Delay(delayMs).ConfigureAwait(false);
            }

            throw new InvalidOperationException("All retry attempts failed.");
        }
    }
}