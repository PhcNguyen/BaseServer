using NServer.Core.Session;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NServer.Core.Interfaces.Session
{
    internal interface ISessionClient
    {
        SessionID Id { get; }
        byte[] Key { get; }
        string IpAddress { get; }
        Socket Socket { get; }
        bool IsConnected { get; }

        Task Connect();
        Task Disconnect();
        Task<bool> Send(object data);
        void UpdateLastActivityTime();
        bool IsSessionTimedOut();
        bool IsSocketDisposed();
    }
}

