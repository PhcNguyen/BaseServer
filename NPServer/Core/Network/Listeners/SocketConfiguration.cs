using NPServer.Infrastructure.Configuration;
using System.Net.Sockets;
using NPServer.Infrastructure.Configuration.Default;

namespace NPServer.Core.Network.Listeners;

public static class SocketConfiguration
{
    /// <summary>
    /// Cấu hình các tùy chọn cho socket.
    /// </summary>
    public static void ConfigureSocket(Socket socket)
    {
        NetworkConfig network = ConfigManager.Instance.GetConfig<NetworkConfig>();

        socket.Blocking = network.Blocking;
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, network.KeepAlive);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, network.ReuseAddress);
    }
}