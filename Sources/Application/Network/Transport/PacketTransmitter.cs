using NETServer.Application.Helper;
using NETServer.Infrastructure.Logging;
using NETServer.Infrastructure.Security;
using NETServer.Infrastructure.Interfaces;
using NETServer.Application.Network.Transport;

using System.Net.Sockets;
using System.Text;

namespace NETServer.Application.Network
{
    internal class PacketTransmitter(Stream stream, byte[] sessionKey, ByteBuffer bufferPool, PacketThrottles packetThrottles) : IPacketTransmitter
    {
        private readonly ByteBuffer _buffer = bufferPool;
        private readonly BufferedStream _stream = new(stream);         // Sử dụng BufferedStream để tối ưu bộ nhớ
        private readonly AesCipher _aesCipher = new(sessionKey);       // Đối tượng xử lý mã hóa/giải mã AES
        private readonly PacketThrottles _throttler = packetThrottles; // Throttler để kiểm soát băng thông

        public bool IsEncrypted { get; private set; } = false;


        private static async ValueTask TcpWriteData(TcpClient tcpClient, byte[] payload)
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
                if (!bufferedStream.CanWrite || payload == null || payload.Length == 0) return;

                byte[] lengthBytes = BitConverter.GetBytes(payload.Length);
                await bufferedStream.WriteAsync(lengthBytes);
                await bufferedStream.WriteAsync(payload);
                await bufferedStream.FlushAsync();
            }
            catch (Exception) { return; }
        }

        // Phương thức chung để xử lý việc gửi dữ liệu
        private async ValueTask<bool> WriteData(Packet packet)
        {
            if (_stream is null || !_stream.CanWrite)
                throw new InvalidOperationException("Stream is not writable. Connection is closed.");

            try
            {
                if (IsEncrypted && _aesCipher != null)
                {
                    packet.UpdatePayload(await _aesCipher.EncryptAsync(packet.Payload));
                }

                // Chuyển gói tin thành mảng byte và gửi
                byte[] packetData = packet.ToByteArray();
                await _stream.WriteAsync(packetData);
                await _throttler.ThrottleSend(packetData.Length); 
                await _stream.FlushAsync();                       

                return true;
            }
            catch (Exception ex)
            {
                NLog.Error($"Error sending data: {ex.Message}");
                return false;
            }
        }

        // Đọc dữ liệu ban đầu (command + length)
        private async ValueTask<(int length, byte[] cmd)> ReadInitialData(CancellationToken cancellationToken)
        {
            if (_stream is null) return (0, Array.Empty<byte>());

            byte[] buffer = _buffer.Rent(5);  // 4 byte cho length, 1 byte cho command
            int bytesRead = await _stream.ReadAsync(buffer, cancellationToken);

            if (bytesRead < buffer.Length)
            {
                _buffer.Return(buffer);
                return (0, Array.Empty<byte>());
            }

            int length = BitConverter.ToInt32(buffer, 0);
            byte[] cmd = [buffer[4]];  // Lấy byte thứ 5 làm Command

            _buffer.Return(buffer);
            return (length, cmd);
        }

        // Nhận dữ liệu bất đồng bộ
        public async ValueTask<Packet?> ReceiveAsync(CancellationToken cancellationToken)
        {
            if (_stream is null || !_stream.CanWrite)
                throw new InvalidOperationException("Stream is not writable. Connection is closed.");

            var buffer = _buffer.Rent(1024);
            await using var dataStream = new MemoryStream();

            try
            {
                var (length, cmd) = await ReadInitialData(cancellationToken);

                if (cmd.Length == 0) return null;

                int bytesRead;
                int totalBytesRead = 0;

                // Đọc dữ liệu vào bộ đệm
                while ((bytesRead = await _stream.ReadAsync(buffer.AsMemory(0, Math.Min(buffer.Length, length - (int)dataStream.Length)), cancellationToken)) > 0)
                {
                    dataStream.Write(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;

                    if (totalBytesRead < length)
                    {
                        if (dataStream.Length >= buffer.Length)
                        {
                            _buffer.Return(buffer);
                            buffer = _buffer.Rent(Math.Min(length - totalBytesRead, 8192));
                        }
                    }

                    if (dataStream.Length >= length) break;

                    // Throttling khi nhận dữ liệu
                    await _throttler.ThrottleReceive(bytesRead);
                }

                byte[] receivedData = dataStream.ToArray();

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

                // Tạo Packet từ dữ liệu nhận được
                return Packet.FromByteArray(receivedData);
            }
            finally
            {
                _buffer.Return(buffer); // Trả lại bộ đệm sau khi sử dụng
            }
        }

        // Gửi dữ liệu qua TCP Client
        public static async ValueTask TcpSend(TcpClient tcpClient, byte[] payload)
        {
            await TcpWriteData(tcpClient, payload);
        }

        public static async ValueTask TcpSend(TcpClient tcpClient, string payload)
        {
            await TcpWriteData(tcpClient, ByteConverter.ToBytes(payload));
        }

        public async ValueTask<bool> SendAsync(byte[] cmd, byte[] payload)
        {
            return await WriteData(new Packet(cmd, payload));
        }

        public async ValueTask<bool> SendAsync(byte[] cmd, string message)
        {
            byte[] payload = Encoding.UTF8.GetBytes(message);
            return await WriteData(new Packet(cmd, payload));
        }

        public async ValueTask<bool> SendAsync(string message)
        {
            byte[] payload = Encoding.UTF8.GetBytes(message);
            return await WriteData(new Packet([0xFF], payload)); // 255
        }

        // Giải phóng tài nguyên
        public void Dispose()
        {
            _aesCipher.Dispose();
            _stream.Dispose();
        }
    }
}
