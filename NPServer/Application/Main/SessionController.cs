﻿using NPServer.Core.Interfaces.Pooling;
using NPServer.Core.Interfaces.Session;
using NPServer.Core.Session;
using NPServer.Infrastructure.Logging;
using NPServer.Infrastructure.Services;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Application.Main
{
    /// <summary>
    /// Lớp điều khiển các phiên làm việc và xử lý gói tin từ người dùng.
    /// </summary>
    internal class SessionController
    {
        private readonly TimeSpan _timeout;
        private readonly CancellationToken _canceltoken;
        private readonly SessionMonitor _sessionMonitor;
        private readonly ISessionManager _sessionManager;
        private readonly PacketController _packetContainer;
        private readonly IMultiSizeBufferPool _multiSizeBuffer;

        /// <summary>
        /// Khởi tạo một <see cref="SessionController"/> mới.
        /// </summary>
        /// <param name="token">Token hủy bỏ cho các tác vụ bất đồng bộ.</param>
        public SessionController(TimeSpan timeout, CancellationToken token)
        {
            _timeout = timeout;
            _canceltoken = token;
            _sessionManager = Singleton.GetInstanceOfInterface<ISessionManager>();
            _multiSizeBuffer = Singleton.GetInstanceOfInterface<IMultiSizeBufferPool>();

            _packetContainer = new PacketController(_canceltoken);
            _sessionMonitor = new SessionMonitor(_sessionManager, _canceltoken);

            this.Initialization();
        }

        /// <summary>
        /// Khởi tạo các tác vụ ban đầu.
        /// </summary>
        private void Initialization()
        {
            _sessionMonitor.OnInfo += (message) => NPLog.Instance.Info<SessionMonitor>(message);
            _sessionMonitor.OnError += (message, exception) => NPLog.Instance.Error<SessionMonitor>(message, exception);

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
                    NPLog.Instance.Info<SessionController>("Operation was canceled.");
                }
                catch (Exception ex)
                {
                    NPLog.Instance.Error<SessionController>($"Error during initialization: {ex.Message}");
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
            SessionClient session = new(clientSocket, _timeout, _multiSizeBuffer, _canceltoken);

            if (_sessionManager.AddSession(session))
            {
                session.OnInfo += (message) => NPLog.Instance.Info<SessionClient>(message);
                session.OnWarning += (message) => NPLog.Instance.Warning<SessionClient>(message);
                session.OnError += (message, exception) => NPLog.Instance.Error<SessionClient>(message, exception);

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
                    NPLog.Instance.Error<SessionController>($"Error occurred while disconnecting clients: {ex.Message}");
                }
            }

            NPLog.Instance.Info<SessionController>("All connections closed successfully.");
        }
    }
}