using NETServer.Application.Enums;
using NETServer.Application.Helper;
using NETServer.Infrastructure.Interfaces;
using NETServer.Infrastructure.Logging;
using NETServer.Infrastructure.Security;
using System.Buffers;
using System.Net.Sockets;

namespace NETServer.Application.Network
{
    internal class DataTransmitter(int BytesPerSecond) : IDataTransmitter
    {
        private static readonly ArrayPool<byte> bytePool = ArrayPool<byte>.Shared;
        private readonly DataThrottlerHelper _throttler = new(BytesPerSecond); // Throttler để kiểm soát băng thông
        private  AesCipher? _aesCipher;           // Đối tượng xử lý mã hóa/giải mã AES
        private  BufferedStream? _stream;         // Sử dụng BufferedStream để tối ưu bộ nhớ

        public bool IsEncrypted { get; private set; } = false;

        public void Create(Stream stream, byte[] sessionKey)
        {
            ValidationHelper.EnsureNotNull(stream, nameof(stream));
            ValidationHelper.EnsureNotNull(sessionKey, nameof(sessionKey));

            _stream = new BufferedStream(stream);
            _aesCipher = new AesCipher(sessionKey);
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
                using var stream = tcpClient.GetStream();
                using var bufferedStream = new BufferedStream(stream);
                if (!bufferedStream.CanWrite) return;
                if (payload == null || payload.Length == 0) return;

                byte[] lengthBytes = BitConverter.GetBytes(payload.Length);
                await bufferedStream.WriteAsync(lengthBytes);
                await bufferedStream.WriteAsync(payload);
                await bufferedStream.FlushAsync();
            }
            catch (Exception) { return; }
        }

        private async Task<(Cmd command, int expectedLength)> ReadInitialData(byte[] buffer, CancellationToken cancellationToken)
        {
            if (_stream == null) return (default, 0);

            // Đọc 4 byte đầu tiên để lấy chiều dài dữ liệu
            if (await _stream.ReadAsync(buffer.AsMemory(0, 4), cancellationToken) < 4) return (default, 0);

            int expectedLength = BitConverter.ToInt32(buffer, 0);

            // Đọc byte thứ 5 làm Command
            if (await _stream.ReadAsync(buffer.AsMemory(0, 1), cancellationToken) < 1) return (default, 0);

            var command = (Cmd)buffer[0];

            return (command, expectedLength);
        }

        public async Task<(Cmd command, byte[] data)> ReceiveAsync(CancellationToken cancellationToken)
        {
            if (_stream == null || !_stream.CanWrite)
                throw new InvalidOperationException("Stream is not writable. Connection is closed.");

            var buffer = bytePool.Rent(8192);
            await using var data = new MemoryStream();

            try
            {
                var (command, expectedLength) = await ReadInitialData(buffer, cancellationToken);
                if (command == default) return (default, []);

                if (command == Cmd.PING) return (command, []);

                int bytesRead;
                while ((bytesRead = await _stream.ReadAsync(buffer.AsMemory(0, 
                    Math.Min(buffer.Length, expectedLength - (int)data.Length)), cancellationToken)) > 0)
                {
                    data.Write(buffer, 0, bytesRead);
                    if (data.Length >= expectedLength) break;

                    // Giới hạn băng thông khi nhận dữ liệu
                    await _throttler.ThrottleReceive(bytesRead);
                }

                byte[] receivedData = data.ToArray();

                if (IsEncrypted && _aesCipher != null)
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

        public async Task<bool> SendAsync(byte[] payload)
        {
            if (_stream == null || !_stream.CanWrite)
                throw new InvalidOperationException("Stream is not writable. Connection is closed.");

            try
            {
                if (IsEncrypted && _aesCipher != null)
                    payload = await _aesCipher.EncryptAsync(payload);

                byte[] lengthBytes = BitConverter.GetBytes(payload.Length);
                await _stream.WriteAsync(lengthBytes);
                await _stream.WriteAsync(payload);

                // Giới hạn băng thông khi gửi dữ liệu
                await _throttler.ThrottleSend(payload.Length);

                await _stream.FlushAsync();

                return true;
            }
            catch (Exception ex)
            {
                NLog.Error($"Error sending data: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            _aesCipher?.Dispose();
            _stream?.Dispose();
        }
    }
}