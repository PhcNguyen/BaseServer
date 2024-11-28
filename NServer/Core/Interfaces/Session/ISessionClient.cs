using Base.Infrastructure.Services;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Base.Core.Interfaces.Session
{
    internal interface ISessionClient
    {
        UniqueId Id { get; }
        byte[] Key { get; }
        string IpAddress { get; }
        Socket Socket { get; }
        bool IsConnected { get; }

        Task ConnectAsync();
        Task DisconnectAsync();
        Task<bool> SendAsync(object data);
        ValueTask DisposeAsync();

        void UpdateLastActivityTime();
        bool IsSessionTimedOut();
        bool IsSocketInvalid();
    }
}

