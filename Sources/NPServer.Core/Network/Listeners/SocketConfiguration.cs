using System.Net.Sockets;

namespace NPServer.Core.Network.Listeners;

public static class SocketConfiguration
{
    /// <summary>
    /// Cấu hình các tùy chọn cho socket.
    /// </summary>
    public static void ConfigureSocket(Socket socket)
    {
        NetworkConfig settings = new();

        socket.Blocking = settings.Blocking;
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, settings.KeepAlive);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, settings.ReuseAddress);
    }
}