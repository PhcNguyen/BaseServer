using System.Collections.Generic;

using NServer.Infrastructure.Services;

namespace NServer.Core.Interfaces.Session
{
    internal interface ISessionManager
    {
        bool AddSession(ISessionClient session);
        ISessionClient? GetSession(ID36 sessionId);
        bool TryGetSession(ID36 sessionId, out ISessionClient? session);
        bool RemoveSession(ID36 sessionId);
        IEnumerable<ISessionClient> GetAllSessions();
        int Count();
    }
}
