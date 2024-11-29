using System.Threading.Tasks;

namespace NServer.Core.Interfaces.Session
{
    internal interface ISessionMonitor
    {
        Task MonitorSessionsAsync();
        Task CloseConnectionAsync(ISessionClient session);
    }
}
