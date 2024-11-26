using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

using NServer.Core.Interfaces.Session;
using NServer.Infrastructure.Services;

namespace NServer.Core.Session
{
    internal class SessionManager
    {
        private readonly ConcurrentDictionary<ID36, ISessionClient> _activeSessions = new();
        private int _sessionCount = 0;

        /// <summary>
        /// Thêm session mới vào danh sách và cập nhật số lượng session.
        /// </summary>
        public bool AddSession(ISessionClient session)
        {
            // Kiểm tra và thêm session nếu chưa tồn tại
            bool isNewSession = _activeSessions.TryAdd(session.Id, session);

            if (isNewSession)
            {
                // Cập nhật số lượng session khi thêm mới
                Interlocked.Increment(ref _sessionCount);
            }
            return isNewSession;
        }

        /// <summary>
        /// Cập nhật session đã tồn tại vào danh sách.
        /// </summary>
        public bool UpdateSession(ISessionClient session)
        {
            // Cập nhật session nếu đã tồn tại trong danh sách
            bool isUpdated = _activeSessions.ContainsKey(session.Id);
            if (isUpdated)
            {
                // Nếu session đã tồn tại, cập nhật lại session
                _activeSessions[session.Id] = session;
            }
            return isUpdated;
        }

        /// <summary>
        /// Lấy session theo ID.
        /// </summary>
        public ISessionClient? GetSession(ID36 sessionId)
        {
            _activeSessions.TryGetValue(sessionId, out var session);
            return session;
        }

        /// <summary>
        /// Xóa session theo ID.
        /// </summary>
        public bool RemoveSession(ID36 sessionId)
        {
            bool isRemoved = _activeSessions.TryRemove(sessionId, out _);

            // Nếu xóa thành công, giảm số lượng session
            if (isRemoved)
            {
                Interlocked.Decrement(ref _sessionCount);
            }

            return isRemoved;
        }

        /// <summary>
        /// Lấy danh sách tất cả các session.
        /// </summary>
        public IEnumerable<ISessionClient> GetAllSessions()
        {
            return _activeSessions.Values;
        }

        /// <summary>
        /// Lấy số lượng session hiện tại.
        /// </summary>
        public int Count()
        {
            return _sessionCount;  // Trả về số lượng session từ biến đếm
        }
    }
}