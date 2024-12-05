using NServer.Infrastructure.Configuration;
using System.Net.Sockets;

namespace NServer.Core.Network.Listeners;

public static class SocketConfiguration
{
    /// <summary>
    /// Cấu hình các tùy chọn cho socket.
    /// </summary>
    public static void ConfigureSocket(Socket socket)
    {
        socket.Blocking = Setting.Blocking;
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, Setting.KeepAlive);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, Setting.ReuseAddress);
    }
}