using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

using NServer.Interfaces.Core.Network;

namespace NServer.Application.Main
{
    internal class SessionManager
    {
        public readonly ConcurrentDictionary<Guid, ISession> ActiveSessions = new();
        private int _sessionCount = 0; 

        /// <summary>
        /// Thêm hoặc cập nhật session vào danh sách.
        /// </summary>
        public bool AddOrUpdateSession(ISession session)
        {
            bool isNewSession = ActiveSessions.TryAdd(session.Id, session);
            if (!isNewSession)
            {
                // Nếu session đã tồn tại, cập nhật lại session
                ActiveSessions[session.Id] = session;
            }

            // Cập nhật số lượng session
            Interlocked.Increment(ref _sessionCount);
            return true;
        }

        /// <summary>
        /// Lấy session theo ID.
        /// </summary>
        public ISession? GetSession(Guid sessionId)
        {
            ActiveSessions.TryGetValue(sessionId, out var session);
            return session;
        }

        /// <summary>
        /// Kiểm tra xem session có tồn tại không.
        /// </summary>
        public bool TryGetSession(Guid sessionId, out ISession? session)
        {
            return ActiveSessions.TryGetValue(sessionId, out session);
        }

        /// <summary>
        /// Xóa session theo ID.
        /// </summary>
        public bool RemoveSession(Guid sessionId)
        {
            bool isRemoved = ActiveSessions.TryRemove(sessionId, out _);

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
        public IEnumerable<ISession> GetAllSessions()
        {
            return ActiveSessions.Values;
        }

        /// <summary>
        /// Lấy số lượng session hiện tại.
        /// </summary>
        public int GetSessionCount()
        {
            return _sessionCount;  // Trả về số lượng session từ biến đếm
        }
    }
}