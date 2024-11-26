using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using NServer.Application.Handler;

using NServer.Core.Packets;
using NServer.Core.Session;
using NServer.Core.Interfaces.Session;

using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Services;

namespace NServer.Application.Main
{
    /// <summary>
    /// Lớp Container chịu trách nhiệm xử lý các gói tin và quản lý phiên làm việc.
    /// </summary>
    internal class Container
    {
        private readonly CancellationToken _cancellationToken;
        private readonly PacketReceiver _receiverContainer;
        private readonly SessionManager _sessionManager;
        private readonly PacketSender _senderContainer;

        private readonly SemaphoreSlim _signalIncoming = new(0);
        private readonly SemaphoreSlim _signalOutgoing = new(0);

        /// <summary>
        /// Khởi tạo một <see cref="Container"/> mới.
        /// </summary>
        /// <param name="cancellationToken">Token hủy bỏ cho các tác vụ bất đồng bộ.</param>
        public Container(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _receiverContainer = Singleton.GetInstance<PacketReceiver>();
            _sessionManager = Singleton.GetInstance<SessionManager>();
            _senderContainer = Singleton.GetInstance<PacketSender>();

            // Đăng ký sự kiện để kích hoạt signal khi có gói tin đến/đi
            _receiverContainer.PacketAdded += () => _signalIncoming.Release();
            _senderContainer.PacketAdded += () => _signalOutgoing.Release();
        }

        /// <summary>
        /// Xử lý các gói tin đến.
        /// </summary>
        public async Task ProcessIncomingPackets()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                // Chờ tín hiệu có gói tin mới
                await _signalIncoming.WaitAsync(_cancellationToken);

                // Lấy batch gói tin đến
                List<Packet> packetsBatch = _receiverContainer.DequeueBatch(50);

                // Xử lý song song các gói tin
                await Parallel.ForEachAsync(packetsBatch, _cancellationToken, async (packet, token) =>
                {
                    try
                    {
                        Packet responsePacket = await CommandDispatcher.HandleCommand(packet).ConfigureAwait(false);
                        _senderContainer.AddPacket(responsePacket); // Thêm gói tin trả lời vào hàng đợi gửi
                    }
                    catch (Exception ex)
                    {
                        NLog.Instance.Error($"Error processing packet: {ex.Message}");
                    }
                });
            }
        }

        /// <summary>
        /// Xử lý các gói tin đi.
        /// </summary>
        public async Task ProcessOutgoingPackets()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                // Chờ tín hiệu có gói tin mới
                await _signalOutgoing.WaitAsync(_cancellationToken);

                // Lấy batch gói tin đi
                List<Packet> packetsBatch = _senderContainer.DequeueBatch(50);

                // Xử lý song song các gói tin
                await Parallel.ForEachAsync(packetsBatch, _cancellationToken, async (packet, token) =>
                {
                    ISessionClient? session = _sessionManager.GetSession(packet.Id);

                    if (session == null || !session.IsConnected)
                        return;

                    try
                    {
                        session.UpdateLastActivityTime();

                        if (packet.Payload.Length == 0)
                            return;

                        // Gửi gói tin với retry logic
                        await RetryAsync(async () => await session.SendAsync(packet).ConfigureAwait(false), 3, 100).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        NLog.Instance.Error($"Error sending packet: {ex.Message}");
                    }
                });
            }
        }

        /// <summary>
        /// Thực hiện lại hành động không đồng bộ nhiều lần nếu thất bại.
        /// </summary>
        /// <param name="action">Hành động không đồng bộ cần thực hiện.</param>
        /// <param name="maxRetries">Số lần thử lại tối đa.</param>
        /// <param name="delayMs">Thời gian chờ giữa các lần thử lại.</param>
        /// <returns>Trả về true nếu thành công, ngược lại false.</returns>
        private static async Task<bool> RetryAsync(Func<Task<bool>> action, int maxRetries, int delayMs)
        {
            int attempt = 0;
            while (attempt < maxRetries)
            {
                try
                {
                    bool result = await action();  // Lấy giá trị trả về từ hành động
                    if (result) return true;
                }
                catch
                {
                    if (++attempt >= maxRetries)
                        throw;

                    await Task.Delay(delayMs);  // Đợi một chút trước khi thử lại
                }
            }
            return false;  // Nếu tất cả các lần thử đều thất bại
        }

        /// <summary>
        /// Lấy số lượng gói tin đến.
        /// </summary>
        /// <returns>Số lượng gói tin đến.</returns>
        public int GetIncomingPacketCount() => _receiverContainer.Count();

        /// <summary>
        /// Lấy số lượng gói tin đi.
        /// </summary>
        /// <returns>Số lượng gói tin đi.</returns>
        public int GetOutgoingPacketCount() => _senderContainer.Count();
    }
}