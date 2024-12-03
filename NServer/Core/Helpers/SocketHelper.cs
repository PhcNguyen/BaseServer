using System;
using System.Net;
using System.Net.Sockets;

namespace NServer.Core.Helpers;

/// <summary>
/// Lớp helper cho các tác vụ liên quan đến socket.
/// </summary>
public static class SocketHelper
{
    /// <summary>
    /// Gắn kết và bắt đầu lắng nghe trên socket với các kết nối TCP.
    /// </summary>
    /// <param name="listenerSocket">Socket lắng nghe.</param>
    /// <param name="ipAddress">Địa chỉ IP để lắng nghe, nếu null thì sử dụng tất cả địa chỉ.</param>
    /// <param name="port">Cổng để lắng nghe.</param>
    /// <param name="maxConnections">Số lượng kết nối tối đa.</param>
    /// <exception cref="ArgumentOutOfRangeException">Khi port không hợp lệ.</exception>
    /// <exception cref="InvalidOperationException">Khi không thể khởi động socket lắng nghe.</exception>
    public static void BindAndListen(Socket listenerSocket, string? ipAddress, int port, int maxConnections)
    {
        if (port < 0 || port > 65535)
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 0 and 65535.");

        try
        {
            IPAddress parsedIPAddress = string.IsNullOrEmpty(ipAddress) ? IPAddress.Any : NetworkHelper.ParseIPAddress(ipAddress);
            var localEndPoint = new IPEndPoint(parsedIPAddress, port);

            listenerSocket.Bind(localEndPoint);
            listenerSocket.Listen(maxConnections);

            if (port == 0)
            {
                // Trả về thông tin cổng được chọn động
                int selectedPort = ((IPEndPoint)listenerSocket.LocalEndPoint!).Port;
                throw new ArgumentException($"Listening on dynamically selected port: {selectedPort}");
            }
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("Invalid IP address format.", ex);
        }
        catch (SocketException ex)
        {
            throw new InvalidOperationException("Failed to start listening on socket.", ex);
        }
    }

    /// <summary>
    /// Đóng socket và giải phóng tài nguyên.
    /// </summary>
    /// <param name="listenerSocket">Socket lắng nghe cần đóng.</param>
    /// <exception cref="InvalidOperationException">Khi xảy ra lỗi trong quá trình đóng socket.</exception>
    public static void CloseSocket(Socket listenerSocket)
    {
        if (!listenerSocket.IsBound)
            throw new InvalidOperationException("Socket is not bound. Cannot stop listening.");

        try
        {
            if (listenerSocket.Connected)
            {
                listenerSocket.Shutdown(SocketShutdown.Both);
            }

            listenerSocket.Close();
        }
        catch (SocketException ex)
        {
            throw new InvalidOperationException("Error occurred while shutting down the socket.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unexpected error occurred while closing the socket.", ex);
        }
    }
}