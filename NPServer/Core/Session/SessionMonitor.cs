using NPServer.Core.Interfaces.Session;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Core.Session
{
    /// <summary>
    /// Giám sát và quản lý trạng thái của các phiên làm việc.
    /// <para>
    /// Lớp này chịu trách nhiệm giám sát các phiên làm việc, kiểm tra trạng thái kết nối của các
    /// phiên làm việc và đóng các kết nối không hợp lệ hoặc đã hết thời gian.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Khởi tạo một đối tượng giám sát phiên làm việc với bộ quản lý phiên và mã thông báo hủy.
    /// </remarks>
    /// <param name="sessionManager">Quản lý các phiên làm việc.</param>
    /// <param name="cancellationToken">Mã thông báo hủy để kiểm soát việc dừng giám sát.</param>
    public partial class SessionMonitor(ISessionManager sessionManager, CancellationToken cancellationToken)
        : ISessionMonitor
    {
        private readonly ISessionManager _sessionManager = sessionManager;
        private readonly CancellationToken _token = cancellationToken;

        /// <summary>
        /// Sự kiện thông tin.
        /// </summary>
        public event Action<string>? OnInfo;

        /// <summary>
        /// Sự kiện lỗi.
        /// </summary>
        public event Action<string, Exception>? OnError;

        /// <summary>
        /// Giám sát các phiên làm việc không đồng bộ, kiểm tra và đóng các kết nối không hợp lệ.
        /// </summary>
        /// <returns>Task đại diện cho tác vụ giám sát phiên làm việc.</returns>
        public async Task MonitorSessionsAsync()
        {
            while (!_token.IsCancellationRequested)
            {
                foreach (ISessionClient session in _sessionManager.GetAllSessions())
                {
                    try
                    {
                        if (session.IsSessionTimedOut() || session.IsSocketInvalid())
                        {
                            CloseConnection(session);
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke($"Error monitoring session {session.Id}", ex);
                    }
                }

                await Task.Delay(2000, _token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Đóng kết nối của một phiên làm việc nếu kết nối không hợp lệ hoặc đã hết thời gian.
        /// </summary>
        /// <param name="session">Phiên làm việc cần đóng kết nối.</param>
        /// <returns>Task đại diện cho tác vụ đóng kết nối.</returns>
        public void CloseConnection(ISessionClient session)
        {
            try
            {
                _sessionManager.RemoveSession(session.Id);
                session.Disconnect();
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Error closing connection for session {session.Id}", ex);
            }
        }
    }
}