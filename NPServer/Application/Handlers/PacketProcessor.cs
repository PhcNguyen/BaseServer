using NPServer.Core.Interfaces.Communication;
using NPServer.Core.Interfaces.Pooling;
using NPServer.Core.Interfaces.Session;
using NPServer.Infrastructure.Logging;
using System;
using System.Threading;

namespace NPServer.Application.Handlers
{
    /// <summary>
    /// Lớp chịu trách nhiệm xử lý gói tin đến và đi.
    /// </summary>
    internal class PacketProcessor(ISessionManager sessionManager, IPacketPool packetPool)
    {
        private readonly IPacketPool _packetPool = packetPool;
        private readonly ISessionManager _sessionManager = sessionManager;

        /// <summary>
        /// Xử lý gói tin đến.
        /// </summary>
        public void HandleIncomingPacket(IPacket packet, PacketQueue outgoingQueue)
        {
            try
            {
                //IPacket responsePacket = await _commandDispatcher.HandleCommand(packet).ConfigureAwait(false);
                Console.WriteLine("Nhan 1 goi tin nha.");
                outgoingQueue.Enqueue(packet);
                //_packetPool.ReturnPacket(packet);
            }
            catch (Exception ex)
            {
                NPLog.Instance.Error<PacketProcessor>($"[HandleIncomingPacket] Error processing packet: {ex}");
            }
        }

        /// <summary>
        /// Xử lý gói tin đi.
        /// </summary>
        public void HandleOutgoingPacket(IPacket packet)
        {
            if (!_sessionManager.TryGetSession(packet.Id, out var session) || session == null)
                return;

            try
            {
                Console.WriteLine("Xu ly 1 goi tin nha.");

                session.UpdateLastActivityTime();

                if (packet.PayloadData.Length == 0)
                    return;

                RetryAsync(() => session.Network.Send(packet.ToByteArray()), maxRetries: 3, delayMs: 100);

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
}