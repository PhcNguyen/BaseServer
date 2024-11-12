using System.Net.Sockets;

namespace NETServer.Infrastructure.Interfaces
{
    internal interface IClientSession
    {
        Guid Id { get; }
        bool IsConnected { get; }
        string ClientAddress { get; }
        TcpClient TcpClient { get; }
        Stream? ClientStream { get; }
        Task Connect();
        Task Disconnect();
        Task<bool> AuthorizeClientSession();
        void UpdateLastActivityTime();
        bool IsSessionTimedOut();
    }
}

