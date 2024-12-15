using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Core.Network.Listeners;

/// <summary>
/// Lớp UDP socket listener dùng để gửi và nhận dữ liệu qua giao thức UDP.
/// </summary>
public class UdpSocketListener(int maxConnections)
    : SocketListenerBase(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp, maxConnections)
{
    /// <summary>
    /// Bắt đầu lắng nghe các gói UDP đến.
    /// </summary>
    /// <param name="ipAddress">Địa chỉ IP để lắng nghe, nếu null thì sử dụng tất cả địa chỉ.</param>
    /// <param name="port">Cổng để lắng nghe.</param>
    public override void StartListening(string? ipAddress, int port)
    {
        // Bind socket để nhận dữ liệu từ bất kỳ địa chỉ nào
        var endPoint = new IPEndPoint(
            string.IsNullOrWhiteSpace(ipAddress) ? IPAddress.Any : IPAddress.Parse(ipAddress),
            port);

        base.ListenerSocket.Bind(endPoint);
    }

    /// <summary>
    /// Gửi dữ liệu đến một endpoint cụ thể.
    /// </summary>
    /// <param name="data">Dữ liệu cần gửi.</param>
    /// <param name="remoteEndPoint">Endpoint nhận dữ liệu.</param>
    public void Send(byte[] data, EndPoint remoteEndPoint)
    {
        try
        {
            base.ListenerSocket.SendTo(data, remoteEndPoint);
        }
        catch (SocketException ex)
        {
            throw new InvalidOperationException("Error occurred while sending data via UDP.", ex);
        }
    }

    /// <summary>
    /// Gửi dữ liệu bất đồng bộ đến một endpoint cụ thể.
    /// </summary>
    /// <param name="data">Dữ liệu cần gửi.</param>
    /// <param name="remoteEndPoint">Endpoint nhận dữ liệu.</param>
    public async Task SendAsync(byte[] data, EndPoint remoteEndPoint)
    {
        try
        {
            await base.ListenerSocket.SendToAsync(data, SocketFlags.None, remoteEndPoint);
        }
        catch (SocketException ex)
        {
            throw new InvalidOperationException("Error occurred while sending data asynchronously via UDP.", ex);
        }
    }

    /// <summary>
    /// Nhận dữ liệu từ một client.
    /// </summary>
    /// <returns>Dữ liệu nhận được và endpoint của client.</returns>
    public (byte[] Data, EndPoint RemoteEndPoint) Receive()
    {
        try
        {
            var buffer = new byte[base.MaxConnections];
            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0) as EndPoint;

            var receivedBytes = base.ListenerSocket.ReceiveFrom(buffer, ref remoteEndPoint);
            var data = new byte[receivedBytes];
            Array.Copy(buffer, data, receivedBytes);

            return (data, remoteEndPoint);
        }
        catch (SocketException ex)
        {
            throw new InvalidOperationException("Error occurred while receiving data via UDP.", ex);
        }
    }

    /// <summary>
    /// Nhận dữ liệu bất đồng bộ từ một client.
    /// </summary>
    /// <param name="token">Token hủy để dừng nhận khi cần thiết.</param>
    /// <returns>Dữ liệu nhận được và endpoint của client.</returns>
    public async Task<(byte[] Data, EndPoint RemoteEndPoint)> ReceiveAsync(CancellationToken token)
    {
        try
        {
            var buffer = new byte[base.MaxConnections];
            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0) as EndPoint;

            var result = await base.ListenerSocket.ReceiveFromAsync(buffer, SocketFlags.None, remoteEndPoint, token);
            var data = new byte[result.ReceivedBytes];
            Array.Copy(buffer, data, result.ReceivedBytes);

            return (data, result.RemoteEndPoint);
        }
        catch (SocketException ex)
        {
            throw new InvalidOperationException("Error occurred while receiving data asynchronously via UDP.", ex);
        }
    }

    /// <summary>
    /// Đặt lại socket listener.
    /// </summary>
    public void ResetListener()
    {
        StopListening();
        Dispose();

        ResetListenerSocket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }
}
