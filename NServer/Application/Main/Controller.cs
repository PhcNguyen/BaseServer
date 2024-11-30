using System;
using System.Linq;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;

using NServer.Core.Session;
using NServer.Core.Packets.Utils;
using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Services;
using NServer.Core.Interfaces.Session;
using NServer.Core.Interfaces.Packets;

namespace NServer.Application.Main
{
    /// <summary>
    /// Lớp điều khiển các phiên làm việc và xử lý gói tin từ người dùng.
    /// </summary>
    internal class Controller
    {
        private readonly CancellationToken _token;
        private readonly SessionMonitor _sessionMonitor;
        private readonly ISessionManager _sessionManager;
        private readonly IPacketIncoming _packetIncoming;
        private readonly PacketContainer _packetContainer;

        /// <summary>
        /// Khởi tạo một <see cref="Controller"/> mới.
        /// </summary>
        /// <param name="token">Token hủy bỏ cho các tác vụ bất đồng bộ.</param>
        public Controller(CancellationToken token)
        {
            _token = token;
            _packetIncoming = Singleton.GetInstance<IPacketIncoming>();
            _sessionManager = Singleton.GetInstanceOfInterface<ISessionManager>();

            _packetContainer = new PacketContainer(_token);
            _sessionMonitor = new SessionMonitor(_sessionManager, _token);

            this.Initialization();
        }

        /// <summary>
        /// Khởi tạo các tác vụ ban đầu.
        /// </summary>
        private void Initialization()
        {
            Task monitorSessionsTask = _sessionMonitor.MonitorSessionsAsync();
            Task processIncomingPacketsTask = _packetContainer.ProcessIncomingPackets();
            Task processOutgoingPacketsTask = _packetContainer.ProcessOutgoingPackets();

            Task.Run(async () =>
            {
                try
                {
                    await Task.WhenAll(monitorSessionsTask, processIncomingPacketsTask, processOutgoingPacketsTask).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    NLog.Instance.Info("Operation was canceled.");
                }
                catch (Exception ex)
                {
                    NLog.Instance.Error($"Error during initialization: {ex.Message}");
                }

            }, _token);
        }

        private void ProcessFunc(UniqueId id, byte[] data)
        {
            if (PacketValidation.IsValidPacket(data)) return;
            IPacket packet = PacketExtensions.FromByteArray(data);
            packet.SetId(id);

            _packetIncoming.AddPacket(packet);
        }
            
        /// <summary>
        /// Lấy số lượng phiên đang hoạt động.
        /// </summary>
        public int ActiveSessions() => _sessionManager.Count();

        /// <summary>
        /// Chấp nhận kết nối từ client mới.
        /// </summary>
        /// <param name="clientSocket">Cổng kết nối của client.</param>
        public async Task AcceptClientAsync(Socket clientSocket)
        {
            SessionClient session = new(clientSocket);

            if (_sessionManager.AddSession(session))
            {
                await session.ConnectAsync().ConfigureAwait(false);
                session.Receive(this.ProcessFunc);
                return;
            }

            await session.DisposeAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Đóng tất cả kết nối của người dùng.
        /// </summary>
        public async ValueTask DisconnectAllClientsAsync()
        {
            var closeTasks = _sessionManager.GetAllSessions()
                .Where(session => session.IsConnected)
                .Select(async session => await _sessionMonitor.CloseConnectionAsync(session).ConfigureAwait(false))
                .ToList(); // Chuyển sang List để kiểm soát số lượng task đồng thời

            while (closeTasks.Count != 0)
            {
                var batch = closeTasks.Take(10).ToList(); // Giới hạn tối đa 10 kết nối cùng lúc
                closeTasks = closeTasks.Skip(10).ToList();

                await Task.WhenAll(batch).ConfigureAwait(false);
            }

            NLog.Instance.Info("All connections closed successfully.");
        }
    }
}