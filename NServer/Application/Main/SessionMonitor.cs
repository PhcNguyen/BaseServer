using NServer.Infrastructure.Logging;
using NServer.Interfaces.Core.Network;

namespace NServer.Application.Main
{
    internal class SessionMonitor(SessionManager sessionManager)
    {
        private readonly SessionManager _sessionManager = sessionManager;

        /// <summary>
        /// Giám sát và kiểm tra trạng thái kết nối của các phiên làm việc.
        /// </summary>
        /// <param name="cancellationToken">Token hủy bỏ cho tác vụ.</param>
        public async Task MonitorSessionsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var session in _sessionManager.GetAllSessions())
                {
                    if (session.IsSessionTimedOut())
                    {
                        await CloseConnectionAsync(session);
                    }
                }

                await Task.Delay(2000, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Đóng kết nối của một phiên làm việc cụ thể.
        /// </summary>
        /// <param name="session">Phiên làm việc cần đóng kết nối.</param>
        public async Task CloseConnectionAsync(ISession session)
        {
            if (session == null) return;

            try
            {
                // Xóa session khỏi manager
                _sessionManager.RemoveSession(session.Id); 
                await session.Disconnect();
            }
            catch (Exception e)
            {
                NLog.Error($"Error while closing connection for session {session.Id}: {e}");
            }
        }
    }
}