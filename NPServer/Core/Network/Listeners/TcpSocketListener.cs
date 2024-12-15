using NPServer.Core.Helpers;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NPServer.Core.Network.Listeners;

public class TcpSocketListener(int maxConnections) 
    : SocketListenerBase(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, maxConnections)
{
    public override void StartListening(string? ipAddress, int port)
    {
        SocketHelper.BindAndListen(base.ListenerSocket, ipAddress, port, base.MaxConnections);
    }

    public void ResetListener()
    {
        StopListening();
        Dispose();

        // Gọi phương thức khởi tạo lại từ lớp cha
        ResetListenerSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

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
    /// Chấp nhận kết nối client một cách bất đồng bộ.
    /// </summary>
    /// <param name="token">Token hủy để dừng việc chấp nhận kết nối khi cần thiết.</param>
    /// <returns>Socket của client được chấp nhận.</returns>
    /// <exception cref="ObjectDisposedException">Nếu socket đã bị đóng trong quá trình Accept.</exception>
    /// <exception cref="OperationCanceledException">Nếu thao tác bị hủy bởi token.</exception>
    /// <exception cref="Exception">Nếu có lỗi không mong muốn khác xảy ra.</exception>
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