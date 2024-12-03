using System;
using System.Net.Sockets;

namespace NServer.Core.Network.IO;

/// <summary>
/// Lớp này quản lý việc gửi dữ liệu bất đồng bộ qua socket.
/// </summary>
public class SocketWriter(Socket socket) : IDisposable
{
    private bool _disposed = false;
    private readonly SocketAsyncEventArgs _sendEventArgs = new();
    private readonly Socket _socket = socket ?? throw new ArgumentNullException(nameof(socket));

    public static void OnCompleted(object? sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError != SocketError.Success)
            throw new SocketException((int)e.SocketError);
    }

    /// <summary>
    /// Gửi dữ liệu qua socket một cách đồng bộ.
    /// </summary>
    /// <param name="data">Dữ liệu cần gửi.</param>
    /// <returns>Trả về True nếu gửi thành công, ngược lại False.</returns>
    public bool Send(byte[] data)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(SocketWriter));

        ArgumentNullException.ThrowIfNull(data);

        _sendEventArgs.SetBuffer(data, 0, data.Length);
        _sendEventArgs.Completed += OnCompleted;

        try
        {
            // Kiểm tra xem có gửi dữ liệu đồng bộ ngay lập tức được không
            if (!_socket.SendAsync(_sendEventArgs))
            {
                // Gửi xong ngay lập tức
                return _sendEventArgs.SocketError == SocketError.Success;
            }

            // Nếu không thể gửi ngay lập tức, chờ callback hoàn thành
            return _sendEventArgs.SocketError == SocketError.Success;
        }
        finally
        {
            _sendEventArgs.Completed -= OnCompleted;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);  // Violates rule
    }

    /// <summary>
    /// Giải phóng tài nguyên khi đối tượng không còn sử dụng.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            _sendEventArgs.Dispose();
            _socket.Dispose();
        }
        catch
        {
            // Xử lý bất kỳ ngoại lệ nào khi dispose
        }
    }
}