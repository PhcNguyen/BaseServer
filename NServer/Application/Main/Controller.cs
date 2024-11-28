using System;
using System.Linq;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;

using Base.Core.Session;
using Base.Infrastructure.Logging;
using Base.Infrastructure.Services;

namespace Base.Application.Main
{
    /// <summary>
    /// Lớp điều khiển các phiên làm việc và xử lý gói tin từ người dùng.
    /// </summary>
    internal class Controller
    {
        private readonly CancellationToken _token;
        private readonly SessionManager _sessionManager;
        private readonly SessionMonitor _sessionMonitor;
        private readonly PacketContainer _packetContainer;

        /// <summary>
        /// Khởi tạo một <see cref="Controller"/> mới.
        /// </summary>
        /// <param name="cancellationToken">Token hủy bỏ cho các tác vụ bất đồng bộ.</param>
        public Controller(CancellationToken cancellationToken)
        {
            _token = cancellationToken;
            _sessionManager = Singleton.GetInstance<SessionManager>();

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
                catch (Exception ex)
                {
                    NLog.Instance.Error($"Error during initialization: {ex.Message}");
                }
            }, _token);
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

            if (session.Authentication())
            {
                _sessionManager.AddSession(session);

                await session.ConnectAsync().ConfigureAwait(false);
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