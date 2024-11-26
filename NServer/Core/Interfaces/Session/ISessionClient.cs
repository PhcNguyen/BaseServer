using NServer.Infrastructure.Services;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NServer.Core.Interfaces.Session
{
    internal interface ISessionClient
    {
        ID36 Id { get; }
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

