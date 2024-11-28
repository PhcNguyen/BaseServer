using System.Threading.Tasks;

namespace Base.Core.Interfaces.Session
{
    internal interface ISessionMonitor
    {
        Task MonitorSessionsAsync();
        Task CloseConnectionAsync(ISessionClient session);
    }
}
