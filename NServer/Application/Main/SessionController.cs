using System;
using System.Linq;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;

using NServer.Core.Session;
using NServer.Core.Packets;
using NServer.Application.Handler;
using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Services;
using NServer.Core.Network.BufferPool;
using NServer.Core.Network.Firewall;
using NServer.Core.Interfaces.Network;

namespace NServer.Application.Main
{
    /// <summary>
    /// Lớp điều khiển các phiên làm việc và xử lý gói tin từ người dùng.
    /// </summary>
    internal class SessionController
    {
        private readonly SemaphoreSlim _semaphore = new(100); // Giới hạn số lượng task đồng thời

        private readonly MultiSizeBuffer _multiSizeBuffer = Singleton.GetInstance<MultiSizeBuffer>();
        private readonly PacketReceiver _packetReceiver = Singleton.GetInstance<PacketReceiver>();

        private readonly CancellationToken _cancellationToken;
        private readonly SessionMonitor _sessionMonitor;
        private readonly SessionManager _sessionManager;

        /// <summary>
        /// Khởi tạo một <see cref="SessionController"/> mới.
        /// </summary>
        /// <param name="cancellationToken">Token hủy bỏ cho các tác vụ bất đồng bộ.</param>
        public SessionController(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _multiSizeBuffer.AllocateBuffers();

            _sessionManager = new SessionManager();
            _sessionMonitor = new SessionMonitor(_sessionManager);

            this.Initialization();
        }

        private void Initialization()
        {
            _ = Task.Run(async () => await _sessionMonitor.MonitorSessionsAsync(_cancellationToken), _cancellationToken);
            _ = Task.Run(async () => await HandleIncomingPacketsAsync(_cancellationToken), _cancellationToken);
        }

        public int ActiveSessions() => _sessionManager.GetSessionCount();

        /// <summary>
        /// Chấp nhận kết nối từ client mới.
        /// </summary>
        /// <param name="clientSocket">Cổng kết nối của client.</param>
        public async Task AcceptClientAsync(Socket clientSocket)
        {
            SessionClient session = new(clientSocket);

            if (session.Authentication())
            {
                // Thêm hoặc cập nhật session
                _sessionManager.AddSession(session);

                await session.Connect();
                return;
            }

            session.Dispose();
        }

        /// <summary>
        /// Quản lý và xử lý các gói tin từ tất cả người dùng.
        /// </summary>
        /// <param name="cancellationToken">Token hủy bỏ cho tác vụ.</param>
        private async Task HandleIncomingPacketsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Lấy 50 gói tin một lần
                var packetsBatch = _packetReceiver.DequeueBatch(50);

                // Tạo danh sách các tác vụ xử lý cho mỗi gói tin
                var tasks = packetsBatch.Select(async packet =>
                {
                    await _semaphore.WaitAsync(cancellationToken); // Chờ tới lượt nếu vượt quá giới hạn

                    try
                    {
                        if (packet != null && packet?.Command != null)
                        {
                            // Xử lý gói tin
                            await HandlePacketsAsync(packet, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        NLog.Instance.Error($"Error processing packet: {ex.Message}");
                    }
                    finally
                    {
                        _semaphore.Release(); // Giải phóng chỗ cho các tác vụ khác
                    }
                }).ToList();

                // Chờ tất cả các tác vụ hoàn thành
                await Task.WhenAll(tasks);

                // Nếu có gói tin, nhường quyền điều khiển ngay lập tức
                if (_packetReceiver.Count() > 0)
                {
                    await Task.Yield(); // Nhường quyền điều khiển ngay lập tức
                }
                else
                {
                    await Task.Delay(10, cancellationToken); // Chỉ chờ nếu hàng đợi trống
                }
            }
        }

        /// <summary>
        /// Xử lý các gói tin trong một kênh cho người dùng.
        /// </summary>
        /// <param name="cancellationToken">Token hủy bỏ cho tác vụ.</param>
        private async Task HandlePacketsAsync(Packet packet, CancellationToken cancellationToken)
        {
            if (!_sessionManager.TryGetSession(packet.Id, out var session)) return;
            if (session == null) return;
            if (!session.IsConnected) return;

            try
            {
                await CommandDispatcher.HandleCommand(session, packet, cancellationToken).ConfigureAwait(false);
                session?.UpdateLastActivityTime();
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Error processing packet for session {session?.IpAddress}: {ex.Message}");
            }
        }

        /// <summary>
        /// Đóng tất cả kết nối của người dùng.
        /// </summary>
        public async ValueTask DisconnectAllClientsAsync()
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

            NLog.Instance.Info("All connections closed successfully.");
        }
    }
}