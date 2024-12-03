using NServer.Core.Interfaces.Session;
using NServer.Core.Session;
using NServer.Infrastructure.Configuration;
using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Services;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NServer.Application.Main
{
    /// <summary>
    /// Lớp điều khiển các phiên làm việc và xử lý gói tin từ người dùng.
    /// </summary>
    internal class SessionController
    {
        private readonly CancellationToken _token;
        private readonly SessionMonitor _sessionMonitor;
        private readonly ISessionManager _sessionManager;
        private readonly PacketContainer _packetContainer;

        /// <summary>
        /// Khởi tạo một <see cref="SessionController"/> mới.
        /// </summary>
        /// <param name="token">Token hủy bỏ cho các tác vụ bất đồng bộ.</param>
        public SessionController(CancellationToken token)
        {
            _token = token;
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
                    NLog.Instance.Info<SessionController>("Operation was canceled.");
                }
                catch (Exception ex)
                {
                    NLog.Instance.Error<SessionController>($"Error during initialization: {ex.Message}");
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
        public void AcceptClient(Socket clientSocket)
        {
            SessionClient session = new(clientSocket, Setting.Timeout);

            if (_sessionManager.AddSession(session))
            {
                session.Connect();
                session.Network.DataReceived += data =>
                {
                    _packetContainer.EnqueueIncomingPacket(session.Id, data);
                };

                return;
            }

            session.Dispose();
        }

        /// <summary>
        /// Đóng tất cả kết nối của người dùng.
        /// </summary>
        public async ValueTask DisconnectAllClientsAsync()
        {
            var closeTasks = _sessionManager.GetAllSessions()
                .Where(session => session.IsConnected)
                .Select(session =>
                {
                    _sessionMonitor.CloseConnection(session);
                    return Task.CompletedTask;
                })
                .ToList();

            var batchSize = 10;
            while (closeTasks.Count != 0)
            {
                var batch = closeTasks.Take(batchSize).ToList();
                closeTasks.RemoveRange(0, batch.Count);

                try
                {
                    await Task.WhenAll(batch).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    NLog.Instance.Error<SessionController>($"Error occurred while disconnecting clients: {ex.Message}");
                }
            }

            NLog.Instance.Info<SessionController>("All connections closed successfully.");
        }
    }
}