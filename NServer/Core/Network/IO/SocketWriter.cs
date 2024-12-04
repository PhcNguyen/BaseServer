using NServer.Core.Interfaces.BufferPool;
using System;
using System.Net.Sockets;

namespace NServer.Core.Network.IO;

/// <summary>
/// Lớp này quản lý việc gửi dữ liệu bất đồng bộ qua socket.
/// </summary>
public class SocketWriter(Socket socket, IMultiSizeBuffer multiSizeBuffer) : IDisposable
{
    private bool _disposed = false;
    private readonly SocketAsyncEventArgs _sendEventArgs = new();
    private readonly Socket _socket = socket ?? throw new ArgumentNullException(nameof(socket));
    private readonly IMultiSizeBuffer _multiSizeBuffer = multiSizeBuffer ?? throw new ArgumentNullException(nameof(multiSizeBuffer));

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

        byte[] buffer = _multiSizeBuffer.RentBuffer(data.Length);

        try
        {
            Array.Copy(data, buffer, data.Length);

            _sendEventArgs.SetBuffer(buffer, 0, data.Length);
            _sendEventArgs.Completed += OnCompleted;

            if (!_socket.SendAsync(_sendEventArgs))
            {
                return _sendEventArgs.SocketError == SocketError.Success;
            }

            return _sendEventArgs.SocketError == SocketError.Success;
        }
        finally
        {
            _sendEventArgs.Completed -= OnCompleted;
            _multiSizeBuffer.ReturnBuffer(buffer);
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