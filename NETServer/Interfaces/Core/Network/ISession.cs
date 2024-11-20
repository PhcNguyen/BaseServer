using System.Net.Sockets;

namespace NETServer.Interfaces.Core.Network
{
    internal interface ISession
    {
        Guid Id { get; }
        byte[] Key { get; }
        string Ip { get; }
        Socket Socket { get; }
        bool IsConnected { get; }

        Task Connect();
        Task Disconnect();
        ValueTask<bool> Send(object data);
        void UpdateLastActivityTime();
        bool IsSessionTimedOut();
    }
}

