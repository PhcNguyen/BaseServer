using System.Collections.Generic;

using NServer.Infrastructure.Services;

namespace NServer.Core.Interfaces.Session
{
    internal interface ISessionManager
    {
        bool AddSession(ISessionClient session);
        ISessionClient? GetSession(UniqueId sessionId);
        bool TryGetSession(UniqueId sessionId, out ISessionClient? session);
        bool RemoveSession(UniqueId sessionId);
        IEnumerable<ISessionClient> GetAllSessions();
        int Count();
    }
}
