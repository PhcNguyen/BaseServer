using System;
using System.Threading;
using System.Threading.Tasks;

using NServer.Infrastructure.Logging;
using NServer.Core.Interfaces.Session;

namespace NServer.Core.Session
{
    internal class SessionMonitor(SessionManager sessionManager)
    {
        private readonly SessionManager _sessionManager = sessionManager;

        /// <summary>
        /// Giám sát kết nối các phiên làm việc.
        /// </summary>
        public async Task MonitorSessionsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var session in _sessionManager.GetAllSessions())
                {
                    try
                    {
                        if (session.IsSessionTimedOut() || session.IsSocketDisposed())
                        {
                            await CloseConnectionAsync(session);
                        }
                    }
                    catch (Exception ex)
                    {
                        NLog.Instance.Error($"Error monitoring session {session.Id}: {ex.Message}");
                    }
                }

                await Task.Delay(2000, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Đóng kết nối phiên làm việc.
        /// </summary>
        public async Task CloseConnectionAsync(ISessionClient session)
        {
            try
            {
                _sessionManager.RemoveSession(session.Id);
                await session.Disconnect();
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Error closing connection for session {session.Id}: {ex.Message}");
            }
        }
    }
}