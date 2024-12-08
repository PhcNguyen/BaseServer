using NPServer.Infrastructure.Services;
using System.Collections.Generic;

namespace NPServer.Core.Interfaces.Session
{
    public interface ISessionManager
    {
        bool AddSession(ISessionClient session);

        ISessionClient? GetSession(UniqueId sessionId);

        bool TryGetSession(UniqueId sessionId, out ISessionClient? session);

        bool RemoveSession(UniqueId sessionId);

        IEnumerable<ISessionClient> GetAllSessions();

        int Count();
    }
}