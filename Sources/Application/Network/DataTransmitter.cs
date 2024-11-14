using NETServer.Application.Handlers;
using NETServer.Application.Helper;
using NETServer.Infrastructure.Interfaces;
using NETServer.Infrastructure.Logging;
using NETServer.Infrastructure.Security;

using System.Buffers;
using System.Net.Sockets;

namespace NETServer.Application.Network
{
    internal class DataTransmitter : IDataTransmitter
    {
        private static readonly ArrayPool<byte> bytePool = ArrayPool<byte>.Shared;
        private readonly AesCipher _aesCipher;          // Đối tượng xử lý mã hóa/giải mã AES
        private readonly BufferedStream _stream;        // Sử dụng BufferedStream để tối ưu bộ nhớ
        private readonly BandwidthThrottler _throttler; // Throttler để kiểm soát băng thông

        public bool IsEncrypted { get; private set; } = false;

        // Khởi tạo với tốc độ băng thông tối đa (bytes per second)
        public DataTransmitter(Stream stream, byte[] key, int maxBytesPerSecond)
        {
            ValidateParameters(stream, key);

            _stream = new BufferedStream(stream);
            _aesCipher = new AesCipher(key);
            _throttler = new BandwidthThrottler(maxBytesPerSecond);  // Tạo đối tượng throttler
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

        private async Task<(Command command, int expectedLength)> ReadInitialDataAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            // Đọc 4 byte đầu tiên để lấy chiều dài dữ liệu
            if (await _stream.ReadAsync(buffer.AsMemory(0, 4), cancellationToken) < 4) return (default, 0);

            int expectedLength = BitConverter.ToInt32(buffer, 0);

            // Đọc byte thứ 5 làm Command
            if (await _stream.ReadAsync(buffer.AsMemory(0, 1), cancellationToken) < 1) return (default, 0);

            var command = (Command)buffer[0];

            return (command, expectedLength);
        }

        public async Task<(Command command, byte[] data)> ReceiveAsync(CancellationToken cancellationToken)
        {
            if (!_stream.CanRead)
                throw new InvalidOperationException("Stream is not writable. Connection is closed.");

            var buffer = bytePool.Rent(8192);
            await using var data = new MemoryStream();

            try
            {
                var (command, expectedLength) = await ReadInitialDataAsync(buffer, cancellationToken);
                if (command == default) return (default, Array.Empty<byte>());

                if (command == Command.PING) return (command, Array.Empty<byte>());

                int bytesRead;
                while ((bytesRead = await _stream.ReadAsync(buffer.AsMemory(0, Math.Min(buffer.Length, expectedLength - (int)data.Length)), cancellationToken)) > 0)
                {
                    data.Write(buffer, 0, bytesRead);
                    if (data.Length >= expectedLength) break;

                    // Giới hạn băng thông khi nhận dữ liệu
                    await _throttler.ThrottleReceive(bytesRead);
                }

                byte[] receivedData = data.ToArray();

                if (IsEncrypted)
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
            if (!_stream.CanWrite)
                throw new InvalidOperationException("Stream is not writable. Connection is closed.");

            try
            {
                if (IsEncrypted)
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
    }
}