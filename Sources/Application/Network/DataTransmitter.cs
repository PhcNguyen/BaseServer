using NETServer.Infrastructure.Security;
using NETServer.Application.Handlers;
using NETServer.Logging;

using System.Net.Sockets;
using System.Buffers;

namespace NETServer.Application.Network;

internal class DataTransmitter
{
    private readonly AesCipher _aesCipher;  // Đối tượng xử lý mã hóa/giải mã AES
    private readonly BufferedStream _stream; // Sử dụng BufferedStream để tối ưu bộ nhớ

    public byte[] KeyAes;
    private bool isEncrypted = false;

    private static ArrayPool<byte> bytePool = ArrayPool<byte>.Shared; // Pool bộ nhớ

    public DataTransmitter(Stream stream)
    {
        _aesCipher = new AesCipher(256);
        this.KeyAes = _aesCipher.Key;

        // Bao bọc stream trong BufferedStream
        _stream = new BufferedStream(stream);
    }

    public static async Task Send(TcpClient tcpClient, byte[] payload)
    {
        if (tcpClient == null || !tcpClient.Connected)
        {
            NLog.Error("TCP client is not connected.");
            return;
        }

        try
        {
            using (var stream = tcpClient.GetStream())
            using (var bufferedStream = new BufferedStream(stream))
            {
                if (!bufferedStream.CanWrite) return;
                if (payload == null || payload.Length == 0) return;

                byte[] lengthBytes = BitConverter.GetBytes(payload.Length);
                await bufferedStream.WriteAsync(lengthBytes, 0, lengthBytes.Length);
                await bufferedStream.WriteAsync(payload, 0, payload.Length);
                await bufferedStream.FlushAsync();
            }
        }
        catch (Exception) { return; }
    }

    public async Task<(Command command, byte[] data)> Receive()
    {
        try
        {
            if (!_stream.CanRead)
            {
                NLog.Error("Stream is not readable.");
                return (default, Array.Empty<byte>());
            }

            // Sử dụng buffer từ ArrayPool để tối ưu bộ nhớ
            var buffer = bytePool.Rent(8192);
            var data = new MemoryStream();

            // Đọc 4 byte đầu tiên để lấy chiều dài dữ liệu
            if (await _stream.ReadAsync(buffer, 0, 4) < 4)
                return (default, Array.Empty<byte>());

            int expectedLength = BitConverter.ToInt32(buffer, 0);

            // Đọc byte thứ 5 làm Command
            if (await _stream.ReadAsync(buffer, 0, 1) < 1)
                return (default, Array.Empty<byte>());

            var command = (Command)buffer[0];

            // Đọc byte tiếp theo để kiểm tra flag mã hóa
            if (await _stream.ReadAsync(buffer, 0, 1) < 1)
                return (default, Array.Empty<byte>());

            isEncrypted = buffer[0] == 1;

            // Đọc tiếp các byte khác
            int bytesRead = await _stream.ReadAsync(buffer, 0, expectedLength - 2); // Đã đọc 2 byte (command và flag)
            if (bytesRead == 0) return (default, Array.Empty<byte>());

            data.Write(buffer, 0, bytesRead);
            byte[] receivedData = data.ToArray();

            // Giải mã nếu cần thiết
            if (isEncrypted)
            {
                receivedData = await _aesCipher.DecryptAsync(receivedData);
            }

            // Trả lại buffer cho pool
            bytePool.Return(buffer);

            return (command, receivedData);
        }
        catch (Exception ex)
        {
            NLog.Error(ex, "Error receiving data.");
            return (default, Array.Empty<byte>());
        }
    }

    public async Task<bool> Send(byte[] payload)
    {
        try
        {
            if (!_stream.CanWrite)
            {
                NLog.Error("Stream is not writable.");
                return false;
            }

            if (payload == null || payload.Length == 0)
            {
                NLog.Error("No data to send.");
                return false;
            }

            // Mã hóa dữ liệu nếu IsEncrypted được bật
            if (isEncrypted)
            {
                payload = await _aesCipher.EncryptAsync(payload);
            }

            // Tính chiều dài tổng cộng
            int totalLength = 1 + payload.Length; // 1 byte flag
            byte[] lengthBytes = BitConverter.GetBytes(totalLength);

            // Gửi 4 byte chứa chiều dài
            await _stream.WriteAsync(lengthBytes, 0, lengthBytes.Length);

            // Gửi byte flag mã hóa
            await _stream.WriteAsync(new byte[] { (byte)(isEncrypted ? 1 : 0) }, 0, 1);

            // Gửi dữ liệu chính
            await _stream.WriteAsync(payload, 0, payload.Length);

            // Đảm bảo dữ liệu đã được ghi xong
            await _stream.FlushAsync();

            return true;
        }
        catch (Exception ex)
        {
            NLog.Error(ex, "Error sending data.");
            return false;
        }
    }
}
