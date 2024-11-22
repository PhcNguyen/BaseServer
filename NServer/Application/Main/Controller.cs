using NServer.Core.Network;
using NServer.Application.Handler;
using NServer.Core.Network.Buffers;
using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Services;

using System.Net.Sockets;
using NServer.Core.Packet.Utils;

namespace NServer.Application.Main
{
    /// <summary>
    /// Lớp điều khiển các phiên làm việc và xử lý gói tin từ người dùng.
    /// </summary>
    internal class Controller
    {
        private readonly SemaphoreSlim _semaphore = new(100); // Giới hạn số lượng task đồng thời

        private readonly CommandHandler _commandHandler = Singleton.GetInstance<CommandHandler>();
        private readonly MultiSizeBuffer _multiSizeBuffer = Singleton.GetInstance<MultiSizeBuffer>();
        private readonly PacketContainer _packetContainer = Singleton.GetInstance<PacketContainer>();
        
        private readonly CancellationToken _cancellationToken;
        private readonly SessionMonitor _sessionMonitor;
        private readonly SessionManager _sessionManager;

        /// <summary>
        /// Khởi tạo một <see cref="Controller"/> mới.
        /// </summary>
        /// <param name="cancellationToken">Token hủy bỏ cho các tác vụ bất đồng bộ.</param>
        public Controller(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _multiSizeBuffer.AllocateBuffers();

            _sessionManager = new SessionManager();
            _sessionMonitor = new SessionMonitor(_sessionManager);

            this.Initialization();
        }

        public void Initialization()
        {
            _ = Task.Run(async () => await _sessionMonitor.MonitorSessionsAsync(_cancellationToken), _cancellationToken);
            _ = Task.Run(async () => await ProcessPacketsAsync(_cancellationToken), _cancellationToken);
        }

        public int ActiveSessions() => _sessionManager.GetSessionCount();

        /// <summary>
        /// Chấp nhận kết nối từ client mới.
        /// </summary>
        /// <param name="clientSocket">Cổng kết nối của client.</param>
        public async Task AcceptClientAsync(Socket clientSocket)
        {
            Session session = new(clientSocket);

            // Thêm hoặc cập nhật session
            _sessionManager.AddOrUpdateSession(session);

            await session.Connect();
        }

        /// <summary>
        /// Quản lý và xử lý các gói tin từ tất cả người dùng.
        /// </summary>
        /// <param name="cancellationToken">Token hủy bỏ cho tác vụ.</param>
        public async Task ProcessPacketsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var tasks = _packetContainer.UsersQueues
                    .Select(async kvp =>
                    {
                        await _semaphore.WaitAsync(cancellationToken);
                        try
                        {
                            await ProcessChannelPacketsAsync(kvp.Key, cancellationToken);
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    })
                    .ToList();

                await Task.WhenAll(tasks);

                await Task.Delay(100, cancellationToken); // Điều chỉnh độ trễ theo nhu cầu
            }
        }

        /// <summary>
        /// Xử lý các gói tin trong một kênh cho người dùng.
        /// </summary>
        /// <param name="userId">ID của người dùng cần xử lý.</param>
        /// <param name="cancellationToken">Token hủy bỏ cho tác vụ.</param>
        private async Task ProcessChannelPacketsAsync(Guid userId, CancellationToken cancellationToken)
        {
            var packetsBatch = _packetContainer.GetPacketsBatch(userId, 100);


            if (!_sessionManager.TryGetSession(userId, out var session) 
                || session == null 
                || !session.IsConnected) return; 

            foreach (var packet in packetsBatch)
            {
                if (packet?.Command == null) continue;

                try
                {
                    await _commandHandler.HandleCommand(session, packet, cancellationToken).ConfigureAwait(false);
                    session?.UpdateLastActivityTime();
                }
                catch (Exception ex)
                {
                    NLog.Error($"Error processing packet for session {session?.Ip}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Đóng tất cả kết nối của người dùng.
        /// </summary>
        public async ValueTask CloseAllConnections()
        {
            var closeTasks = _sessionManager.GetAllSessions()
                .Where(session => session.IsConnected)
                .Select(async session => await _sessionMonitor.CloseConnectionAsync(session))
                .ToList(); // Chuyển sang List để kiểm soát số lượng task đồng thời

            while (closeTasks.Count != 0)
            {
                var batch = closeTasks.Take(10).ToList(); // Giới hạn tối đa 10 kết nối cùng lúc
                closeTasks = closeTasks.Skip(10).ToList();

                await Task.WhenAll(batch).ConfigureAwait(false);
            }

            NLog.Info("All connections closed successfully.");
        }
    }
}