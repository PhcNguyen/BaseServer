using System.Net.Sockets;

namespace NServer.Interfaces.Core.Network
{
    internal interface ISession
    {
        Guid Id { get; }
        byte[] Key { get; }
        string Ip { get; }
        Socket Socket { get; }
        INSocket SocketAsync { get; }
        bool IsConnected { get; }

        Task Connect();
        Task Disconnect();
        bool Send(object data);
        void UpdateLastActivityTime();
        bool IsSessionTimedOut();
        bool IsSocketDisposed();
    }
}

