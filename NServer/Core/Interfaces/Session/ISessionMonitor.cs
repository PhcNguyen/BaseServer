using System.Threading.Tasks;

namespace NServer.Core.Interfaces.Session
{
    public interface ISessionMonitor
    {
        Task MonitorSessionsAsync();

        Task CloseConnectionAsync(ISessionClient session);
    }
}