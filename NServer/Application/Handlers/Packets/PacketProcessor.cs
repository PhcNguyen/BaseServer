using System;
using System.Threading.Tasks;

using NServer.Infrastructure.Logging;
using NServer.Core.Interfaces.Session;
using NServer.Core.Interfaces.Packets;

namespace NServer.Application.Handlers.Packets
{
    /// <summary>
    /// Lớp PacketProcessor chịu trách nhiệm xử lý các gói tin đến và đi.
    /// </summary>
    /// <remarks>
    /// Khởi tạo một đối tượng <see cref="PacketProcessor"/> mới.
    /// </remarks>
    /// <param name="sessionManager">Đối tượng quản lý phiên làm việc.</param>
    internal class PacketProcessor(ISessionManager sessionManager)
    {
        private readonly ISessionManager _sessionManager = sessionManager;

        /// <summary>
        /// Xử lý gói tin đến và thêm gói tin phản hồi vào hàng đợi gửi.
        /// </summary>
        /// <param name="packet">Gói tin đến cần xử lý.</param>
        /// <param name="outgoingQueue">Hàng đợi gửi gói tin.</param>
        public async Task HandleIncomingPacket(IPacket packet, IPacketOutgoing outgoingQueue)
        {
            try
            {
                IPacket responsePacket = await CommandDispatcher.HandleCommand(packet).ConfigureAwait(false);
                outgoingQueue.AddPacket(responsePacket);
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Error processing incoming packet: {ex.Message}");
            }
        }

        /// <summary>
        /// Xử lý gói tin đi.
        /// </summary>
        /// <param name="packet">Gói tin đi cần xử lý.</param>
        public async Task HandleOutgoingPacket(IPacket packet)
        {
            ISessionClient? session = _sessionManager.GetSession(packet.Id);

            if (session == null || !session.IsConnected)
                return;

            try
            {
                session.UpdateLastActivityTime();

                if (packet.Payload.Length == 0)
                    return;

                await RetryAsync(async () => await session.SendAsync(packet).ConfigureAwait(false), 3, 100).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Error sending packet: {ex.Message}");
            }
        }

        /// <summary>
        /// Thực hiện lại hành động không đồng bộ nhiều lần nếu thất bại.
        /// </summary>
        /// <param name="action">Hành động không đồng bộ cần thực hiện.</param>
        /// <param name="maxRetries">Số lần thử lại tối đa.</param>
        /// <param name="delayMs">Thời gian chờ giữa các lần thử lại.</param>
        /// <returns>Nhiệm vụ không đồng bộ đại diện cho quá trình thực hiện lại.</returns>
        private static async Task RetryAsync(Func<Task<bool>> action, int maxRetries, int delayMs)
        {
            int attempt = 0;
            while (attempt < maxRetries)
            {
                try
                {
                    bool result = await action();
                    if (result) return;
                }
                catch
                {
                    if (++attempt >= maxRetries)
                        throw;

                    await Task.Delay(delayMs);
                }
            }
        }
    }
}