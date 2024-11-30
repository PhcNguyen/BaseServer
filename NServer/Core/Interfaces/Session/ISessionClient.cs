using NServer.Infrastructure.Services;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NServer.Core.Interfaces.Session
{
    internal interface ISessionClient
    {
        UniqueId Id { get; }
        byte[] Key { get; }
        string IpAddress { get; }
        bool Authenticator { get; set; }
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

