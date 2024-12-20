using NPServer.Core.Helpers;
using System;
using System.Net.Sockets;

namespace NPServer.Core.Network.Listeners;

/// <summary>
/// Lớp cơ sở cho các trình lắng nghe kết nối (TCP, UDP, HTTP).
/// </summary>
public abstract class SocketListenerBase : IDisposable
{
    protected Socket ListenerSocket { get; private set; }
    protected readonly int MaxConnections;

    /// <summary>
    /// Kiểm tra xem socket có đang lắng nghe hay không.
    /// </summary>
    public bool IsListening => ListenerSocket?.IsBound == true;

    /// <summary>
    /// Khởi tạo một đối tượng <see cref="SocketListenerBase"/>.
    /// </summary>
    /// <param name="addressFamily">Loại địa chỉ socket.</param>
    /// <param name="socketType">Kiểu socket (TCP hoặc UDP).</param>
    /// <param name="protocolType">Giao thức socket.</param>
    /// <param name="maxConnections">Số lượng kết nối tối đa.</param>
    protected SocketListenerBase(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, int maxConnections)
    {
        MaxConnections = maxConnections;
        ListenerSocket = new Socket(addressFamily, socketType, protocolType)
        {
            NoDelay = true,
            ExclusiveAddressUse = false,
            LingerState = new(false, 0)
        };
        SocketConfiguration.ConfigureSocket(ListenerSocket);
    }

    /// <summary>
    /// Bắt đầu lắng nghe các kết nối đến.
    /// </summary>
    /// <param name="ipAddress">Địa chỉ IP để lắng nghe.</param>
    /// <param name="port">Cổng để lắng nghe.</param>
    public abstract void StartListening(string? ipAddress, int port);

    /// <summary>
    /// Dừng việc lắng nghe và đóng socket.
    /// </summary>
    public virtual void StopListening()
    {
        SocketHelper.CloseSocket(this.ListenerSocket);
    }

    /// <summary>
    /// Đặt lại listener, đóng socket và giải phóng tài nguyên.
    /// </summary>
    protected void ResetListenerSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
    {
        ListenerSocket?.Dispose();
        ListenerSocket = new Socket(addressFamily, socketType, protocolType)
        {
            NoDelay = true,
            ExclusiveAddressUse = false,
            LingerState = new(false, 0)
        };
        SocketConfiguration.ConfigureSocket(ListenerSocket);
    }

    /// <summary>
    /// Giải phóng tài nguyên socket.
    /// </summary>
    public void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.ListenerSocket?.Dispose();
        }
    }

    /// <summary>
    /// Giải phóng tài nguyên.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}