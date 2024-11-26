using System.Linq;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;

using NServer.Core.Session;
using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Services;

namespace NServer.Application.Main
{
    /// <summary>
    /// Lớp điều khiển các phiên làm việc và xử lý gói tin từ người dùng.
    /// </summary>
    internal class Controller
    {
        private readonly SessionManager _sessionManager = Singleton.GetInstance<SessionManager>();

        private readonly CancellationToken _cancellationToken;
        private readonly PacketProcessor _packetProcessor;
        private readonly SessionMonitor _sessionMonitor;

        /// <summary>
        /// Khởi tạo một <see cref="Controller"/> mới.
        /// </summary>
        /// <param name="cancellationToken">Token hủy bỏ cho các tác vụ bất đồng bộ.</param>
        public Controller(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            
            _sessionManager = new SessionManager();
            _sessionMonitor = new SessionMonitor(_sessionManager);
            _packetProcessor = new PacketProcessor(_sessionManager, _cancellationToken);

            this.Initialization();
        }

        private void Initialization()
        {
            _ = Task.Run(async () => await _sessionMonitor.MonitorSessionsAsync(_cancellationToken), _cancellationToken);
            _packetProcessor.StartProcessing();
        }

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

                await session.Connect();
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