using NServer.Core.Interfaces.Pooling;
using NServer.Core.Interfaces.Session;
using NServer.Core.Network.IO;
using NServer.Infrastructure.Security;
using System;
using System.Net.Sockets;

namespace NServer.Core.Session.Network;

/// <summary>
/// Quản lý vận chuyển phiên làm việc, bao gồm gửi và nhận dữ liệu qua socket.
/// </summary>
public class SessionNetwork : IDisposable, ISessionNetwork
{
    private bool _disposed = false;

    /// <summary>
    /// Bộ ghi socket để gửi dữ liệu.
    /// </summary>
    public readonly SocketWriter SocketWriter;

    /// <summary>
    /// Bộ đọc socket để nhận dữ liệu.
    /// </summary>
    public readonly SocketReader SocketReader;

    /// <summary>
    /// Sự kiện kích hoạt khi dữ liệu được nhận.
    /// </summary>
    public event Action<byte[]>? DataReceived;

    public bool IsDispose => _disposed;

    /// <summary>
    /// Khởi tạo một thể hiện mới của lớp <see cref="SessionNetwork"/>.
    /// </summary>
    /// <param name="socket">Socket của khách hàng.</param>
    public SessionNetwork(Socket socket, IMultiSizeBufferPool multiSizeBuffer)
    {
        SocketWriter = new(socket, multiSizeBuffer);
        SocketReader = new(socket, multiSizeBuffer);
        SocketReader.DataReceived += OnDataReceived!;
    }

    /// <summary>
    /// Xử lý sự kiện khi nhận dữ liệu từ socket.
    /// </summary>
    /// <param name="sender">Nguồn của sự kiện.</param>
    /// <param name="e">Dữ liệu sự kiện socket nhận.</param>
    private void OnDataReceived(object sender, SocketReceivedEventArgs e)
    {
        bool isValid = Crc32Checksum.VerifyCrc32(e.Data, out byte[]? originalData);

        if (isValid && originalData != null)
        {
            DataReceived?.Invoke(originalData);
        }
    }

    /// <summary>
    /// Gửi dữ liệu đến khách hàng thông qua socket.
    /// </summary>
    /// <param name="data">Dữ liệu cần gửi, có thể là mảng byte, chuỗi hoặc gói tin.</param>
    /// <returns>True nếu gửi thành công, ngược lại False.</returns>
    /// <summary>
    /// Gửi dữ liệu dạng byte[] đến khách hàng thông qua socket.
    /// </summary>
    /// <param name="data">Dữ liệu cần gửi, dạng byte[]</param>
    /// <returns>True nếu gửi thành công, ngược lại False.</returns>
    public bool Send(byte[] data)
    {
        try
        {
            SocketWriter.Send(Crc32Checksum.AddCrc32(data));
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error sending byte array: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gửi dữ liệu dạng chuỗi đến khách hàng thông qua socket.
    /// </summary>
    /// <param name="data">Dữ liệu cần gửi, dạng chuỗi</param>
    /// <returns>True nếu gửi thành công, ngược lại False.</returns>
    public bool Send(string data)
    {
        try
        {
            SocketWriter.Send(Crc32Checksum.AddCrc32(data));
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error sending string: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Giải phóng tài nguyên khi không còn sử dụng.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);  // Giúp ngăn không cho finalizer tự động chạy.
    }

    /// <summary>
    /// Giải phóng tài nguyên khi không còn sử dụng.
    /// </summary>
    /// <param name="disposing">True nếu được gọi từ Dispose(), false nếu được gọi từ finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Giải phóng các đối tượng quản lý thủ công.
            SocketReader.DataReceived -= OnDataReceived!;

            SocketWriter?.Dispose();
            SocketReader?.Dispose();

            _disposed = true;
        }

        // Giải phóng các tài nguyên không phải managed (nếu có).
    }

    // Finalizer (Destructor) trong trường hợp nếu Dispose chưa được gọi.
    ~SessionNetwork()
    {
        Dispose(false);
    }
}