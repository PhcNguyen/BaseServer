using NServer.Core.Interfaces.BufferPool;
using NServer.Core.Interfaces.Session;
using NServer.Core.Services;
using NServer.Core.Session;
using NServer.Infrastructure.Configuration;
using NServer.Infrastructure.Logging;
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
        private readonly CancellationToken _canceltoken;
        private readonly SessionMonitor _sessionMonitor;
        private readonly ISessionManager _sessionManager;
        private readonly PacketContainer _packetContainer;
        private readonly IMultiSizeBuffer _multiSizeBuffer;

        /// <summary>
        /// Khởi tạo một <see cref="SessionController"/> mới.
        /// </summary>
        /// <param name="token">Token hủy bỏ cho các tác vụ bất đồng bộ.</param>
        public SessionController(CancellationToken token)
        {
            _canceltoken = token;
            _sessionManager = Singleton.GetInstanceOfInterface<ISessionManager>();
            _multiSizeBuffer = Singleton.GetInstanceOfInterface<IMultiSizeBuffer>();

            _packetContainer = new PacketContainer(_canceltoken);
            _sessionMonitor = new SessionMonitor(_sessionManager, _canceltoken);

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
            }, _canceltoken);
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
            SessionClient session = new(clientSocket, Setting.Timeout, _multiSizeBuffer);

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