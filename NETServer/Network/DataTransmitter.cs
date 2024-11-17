using NETServer.Network.Packets;
using NETServer.Infrastructure.Helper;
using NETServer.Infrastructure.Logging;
using NETServer.Infrastructure.Interfaces;

using System.Text;
using System.Net.Sockets;

namespace NETServer.Network
{
    internal class DataTransmitter(Guid UID, Stream stream, ByteBuffer bufferPool, PacketThrottles packetThrottles) : IDataTransmitter
    {
        private readonly Guid UID = UID;
        private readonly ByteBuffer _buffer = bufferPool;
        private readonly BufferedStream _stream = new(stream);
        private readonly PacketThrottles _throttler = packetThrottles;

        public bool IsEncrypted { get; private set; } = false;

        private async ValueTask<Packet?> ReadInitialData(CancellationToken cancellationToken)
        {
            // Return null if stream is null
            if (_stream == null) return null;

            // Đọc 4 byte đầu tiên để lấy length
            byte[] buffer = _buffer.Rent(4);
            try
            {
                int bytesRead = await _stream.ReadAsync(buffer.AsMemory(0, 4), cancellationToken);

                // Nếu không đủ 4 byte (để đọc length), trả về null
                if (bytesRead < 4)
                {
                    return null;
                }

                int length = BitConverter.ToInt32(buffer, 0);  // Đọc length từ 4 byte đầu tiên

                if (length > 4)  // Nếu gói tin có kích thước lớn hơn 4 byte
                {
                    byte[] data = _buffer.Rent(length);
                    try
                    {
                        // Copy phần length đã đọc vào fullPacket
                        Buffer.BlockCopy(buffer, 0, data, 0, 4);

                        // Đọc phần còn lại của gói tin (payload)
                        bytesRead = await _stream.ReadAsync(data.AsMemory(4, length - 4), cancellationToken);

                        if (bytesRead < length - 4)
                        {
                            return null;
                        }

                        return PacketExtensions.CreatePacket(data);
                    }
                    finally
                    {
                        _buffer.Return(data);
                    }
                }
            }
            finally
            {
                _buffer.Return(buffer);
            }

            return null;
        }

        // Helper method to send data
        public async Task SendDataAsync(Packet packet)
        {
            if (_stream == null || !_stream.CanWrite)
                throw new InvalidOperationException("Stream is not writable. Connection is closed.");

            try
            {
                await _stream.WriteAsync(packet.ToByteArray());
                await _stream.FlushAsync();

                await _throttler.ThrottleSend(packet.Length); // Throttle bandwidth
            }
            catch (Exception ex)
            {
                NLog.Error($"Error sending data: {ex.Message}");
            }
        }

        // Send data by a specific command and payload (using byte array)
        public async ValueTask<bool> SendAsync(short command, byte[] payload)
        {
            try
            {
                // Send packet using existing SendDataAsync method
                await SendDataAsync(new Packet(UID, command: command, payload: payload));
                return true;
            }
            catch (Exception ex)
            {
                NLog.Error($"SendAsync failed: {ex.Message}");
                return false;
            }
        }

        // Send data by a specific command and string message (converted to byte array)
        public async ValueTask<bool> SendAsync(short command, string message)
        {
            byte[] payload = Encoding.UTF8.GetBytes(message);
            return await SendAsync(command, payload);
        }

        // Receive data and return a Packet
        public async ValueTask<IPacket?> ReceiveAsync(CancellationToken cancellationToken)
        {
            if (_stream == null || !_stream.CanRead)
                throw new InvalidOperationException("Stream is not readable. Connection is closed.");

            // Read initial data (length + command)
            Packet? packet = await ReadInitialData(cancellationToken);

            if (packet?.Length == 0) return null;

            return packet;
        }

        public static async ValueTask TcpSend(TcpClient tcpClient, byte[] payload)
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

        public static async ValueTask TcpSend(TcpClient tcpClient, string message)
        {
            await TcpSend(tcpClient, ByteConverter.ToBytes(message));
        }

        // Dispose resources
        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}