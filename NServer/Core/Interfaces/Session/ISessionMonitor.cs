using System.Threading.Tasks;

namespace NPServer.Core.Interfaces.Session;

public interface ISessionMonitor
{
    Task MonitorSessionsAsync();

    void CloseConnection(ISessionClient session);
}