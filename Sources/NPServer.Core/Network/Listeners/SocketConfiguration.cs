using System.Net.Sockets;

namespace NPServer.Core.Network.Listeners;

/// <summary>
/// Lớp chứa các phương thức cấu hình cho socket.
/// </summary>
public static class SocketConfiguration
{
    /// <summary>
    /// Cấu hình các tùy chọn cho socket như chế độ khóa, và các tùy chọn liên quan đến kết nối mạng.
    /// </summary>
    /// <param name="socket">Socket cần được cấu hình.</param>
    public static void ConfigureSocket(Socket socket)
    {
        NetworkConfig settings = new();

        socket.Blocking = settings.Blocking;
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, settings.KeepAlive);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, settings.ReuseAddress);
    }
}