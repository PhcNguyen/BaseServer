using NETServer.Application.Security;
using NETServer.Application.Handlers;
using NETServer.Logging;

namespace NETServer.Application.Network;

internal class DataTransmitter
{
    private readonly AesCipher _aesCipher;  // Đối tượng xử lý mã hóa/giải mã AES
    private readonly Stream _stream;        // Stream dùng để nhận và gửi dữ liệu

    public byte[] KeyAes;
    private bool isEncrypted = false;

    public DataTransmitter(Stream stream)
    {
        _stream = stream;
        _aesCipher = new AesCipher(256);

        this.KeyAes = _aesCipher.Key;
    }

    public async Task<(Command command, byte[] data)> Receive(CancellationToken cancellationToken)
    {
        try
        {
            if (!_stream.CanRead)
            {
                NLog.Error("Stream is not readable.");
                return (default, Array.Empty<byte>());
            }

            var buffer = new byte[1024];
            var data = new MemoryStream();

            cancellationToken.ThrowIfCancellationRequested();

            // Đọc 4 byte đầu tiên để lấy chiều dài dữ liệu
            if (await _stream.ReadAsync(buffer, 0, 4, cancellationToken) < 4)
                return (default, Array.Empty<byte>()); // Nếu không đủ 4 byte, trả về mảng rỗng

            int expectedLength = BitConverter.ToInt32(buffer, 0);

            // Đọc byte thứ 5 làm Command
            if (await _stream.ReadAsync(buffer, 0, 1, cancellationToken) < 1)
                return (default, Array.Empty<byte>()); // Nếu không có byte nào, trả về mảng rỗng

            var command = (Command)buffer[0];

            // Đọc byte tiếp theo để kiểm tra flag mã hóa
            if (await _stream.ReadAsync(buffer, 0, 1, cancellationToken) < 1)
                return (default, Array.Empty<byte>());

            isEncrypted = buffer[0] == 1;

            while (data.Length < expectedLength - 2) // -2 vì đã đọc command và flag
            {
                cancellationToken.ThrowIfCancellationRequested();

                int bytesRead = await _stream.ReadAsync(buffer, 0, Math.Min(buffer.Length, expectedLength - 2 - (int)data.Length), cancellationToken);
                if (bytesRead == 0) break; // Nếu không còn dữ liệu để đọc thì thoát

                data.Write(buffer, 0, bytesRead); // Ghi dữ liệu vào MemoryStream
            }

            if (isEncrypted)
            {
                // Giải mã dữ liệu nếu flag mã hóa là true
                var decryptedData = await _aesCipher.DecryptAsync(data.ToArray());
                return (command, decryptedData);
            }

            return (command, data.ToArray()); // Nếu không mã hóa, trả về dữ liệu gốc
        }
        catch (OperationCanceledException)
        {
            NLog.Info("Data receive operation was canceled.");
            return (default, Array.Empty<byte>());
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