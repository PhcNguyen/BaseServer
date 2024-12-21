using NPServer.Core.Helpers;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Core.Network.Listeners;

/// <summary>
/// Lớp quản lý lắng nghe socket TCP với khả năng chấp nhận kết nối từ client.
/// </summary>
/// <remarks>
/// Khởi tạo một TcpSocketListener với số kết nối tối đa.
/// </remarks>
/// <param name="maxConnections">Số lượng kết nối tối đa cho listener.</param>
public class TcpSocketListener(int maxConnections) 
    : SocketListenerBase(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, maxConnections)
{

    /// <summary>
    /// Bắt đầu lắng nghe kết nối từ client.
    /// </summary>
    /// <param name="ipAddress">Địa chỉ IP để lắng nghe. Nếu là null, sẽ sử dụng tất cả các giao diện.</param>
    /// <param name="port">Cổng lắng nghe.</param>
    public override void StartListening(string? ipAddress, int port)
    {
        SocketHelper.BindAndListen(base.ListenerSocket, ipAddress, port, base.MaxConnections);
    }

    /// <summary>
    /// Đặt lại listener, dừng lắng nghe và giải phóng tài nguyên.
    /// </summary>
    public void ResetListener()
    {
        StopListening();
        Dispose();

        // Gọi phương thức khởi tạo lại từ lớp cha
        ResetListenerSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    /// <summary>
    /// Chấp nhận kết nối từ client.
    /// </summary>
    /// <returns>Socket của client được chấp nhận.</returns>
    /// <exception cref="InvalidOperationException">Nếu có lỗi trong quá trình chấp nhận kết nối.</exception>
    public Socket? AcceptClient()
    {
        try
        {
            return base.ListenerSocket.Accept();
        }
        catch (ObjectDisposedException ex)
        {
            throw new InvalidOperationException("Socket was closed during Accept operation.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unexpected error occurred while accepting client.", ex);
        }
    }

    /// <summary>
    /// Chấp nhận kết nối từ client một cách bất đồng bộ.
    /// </summary>
    /// <param name="token">Token hủy để dừng việc chấp nhận kết nối khi cần thiết.</param>
    /// <returns>Socket của client được chấp nhận.</returns>
    /// <exception cref="InvalidOperationException">Nếu có lỗi trong quá trình chấp nhận kết nối.</exception>
    public async Task<Socket?> AcceptClientAsync(CancellationToken token)
    {
        try
        {
            return await base.ListenerSocket.AcceptAsync(token);
        }
        catch (ObjectDisposedException ex)
        {
            throw new InvalidOperationException("Socket was closed during Accept operation.", ex);
        }
        catch (OperationCanceledException ex)
        {
            throw new InvalidOperationException("AcceptClientAsync was cancelled due to cancellation token.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unexpected error occurred while accepting client.", ex);
        }
    }
}
