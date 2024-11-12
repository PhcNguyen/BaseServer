using NETServer.Infrastructure.Interfaces;
using NETServer.Infrastructure.Security;
using NETServer.Infrastructure.Logging;
using NETServer.Application.Handlers;

using System.Net.Sockets;
using System.Buffers;

namespace NETServer.Application.Network;

internal class DataTransmitter: IDataTransmitter
{
    private readonly AesCipher _aesCipher;  // Đối tượng xử lý mã hóa/giải mã AES
    private readonly BufferedStream _stream; // Sử dụng BufferedStream để tối ưu bộ nhớ
    private bool isEncrypted = false;

    private static ArrayPool<byte> bytePool = ArrayPool<byte>.Shared; // Pool bộ nhớ

    public DataTransmitter(Stream stream, byte[] key)
    {
        ValidateParameters(stream, key);

        _stream = new BufferedStream(stream);
        _aesCipher = new AesCipher(key);
    }

    private static void ValidateParameters(Stream stream, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        ArgumentNullException.ThrowIfNull(key, nameof(key));
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

    // Tách phần đọc dữ liệu đầu vào (chiều dài, command, flag mã hóa)
    private async Task<(Command command, int expectedLength, bool isEncrypted)> ReadInitialData(byte[] buffer, CancellationToken cancellationToken)
    {
        // Đọc 4 byte đầu tiên để lấy chiều dài dữ liệu
        int readLength = await _stream.ReadAsync(buffer, 0, 4, cancellationToken);
        if (readLength < 4)
        {
            return (default, 0, false);
        }

        int expectedLength = BitConverter.ToInt32(buffer, 0);

        // Đọc byte thứ 5 làm Command
        readLength = await _stream.ReadAsync(buffer, 0, 1, cancellationToken);
        if (readLength < 1)
        {
            return (default, 0, false);
        }

        var command = (Command)buffer[0];

        // Đọc byte tiếp theo để kiểm tra flag mã hóa
        readLength = await _stream.ReadAsync(buffer, 0, 1, cancellationToken);
        if (readLength < 1)
        {
            return (default, 0, false);
        }

        return (command, expectedLength, buffer[0] == 1);
    }

    public async Task<(Command command, byte[] data)> Receive(CancellationToken cancellationToken)
    {
        try
        {
            if (!_stream.CanRead)
            {
                throw new InvalidOperationException("Stream is not readable. Connection is closed.");
            }

            var buffer = bytePool.Rent(8192);
            var data = new MemoryStream();

            try
            {
                var (command, expectedLength, isEncrypted) = await ReadInitialData(buffer, cancellationToken);
                if (command == default)
                {
                    return (default, Array.Empty<byte>());
                }

                // Trả về Pong, không cần dữ liệu.
                if (command == Command.PING)
                {
                    return (command, Array.Empty<byte>());
                }

                // Đọc phần còn lại của dữ liệu
                int bytesRead = await _stream.ReadAsync(buffer, 0, expectedLength - 2, cancellationToken); // Đã đọc 2 byte (command và flag)
                if (bytesRead == 0)
                {
                    return (default, Array.Empty<byte>());
                }

                data.Write(buffer, 0, bytesRead);

                byte[] receivedData = data.ToArray();

                // Giải mã nếu cần thiết
                if (isEncrypted)
                {
                    try
                    {
                        receivedData = await _aesCipher.DecryptAsync(receivedData);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Decryption failed, invalid data.", ex);
                    }
                }

                return (command, receivedData);
            }
            finally
            {
                bytePool.Return(buffer);
            }
        }
        catch (OperationCanceledException)
        {
            return (default, Array.Empty<byte>());
        }
        catch (Exception ex)
        {
            NLog.Error("An error occurred while receiving data: " + ex.Message);
            return (default, Array.Empty<byte>());
        }
    }

    public async Task<bool> Send(byte[] payload)
    {
        try
        {
            if (!_stream.CanWrite)
            {
                throw new InvalidOperationException("Stream is not readable. Connection is closed.");
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
            NLog.Error(ex);
            return false;
        }
    }
}
