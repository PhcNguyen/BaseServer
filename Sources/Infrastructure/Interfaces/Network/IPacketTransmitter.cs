using NETServer.Application.Network.Transport;

namespace NETServer.Infrastructure.Interfaces
{
    internal interface IPacketTransmitter
    {
        bool IsEncrypted { get; }

        ValueTask<bool> SendAsync(string message);
        ValueTask<bool> SendAsync(byte[] cmd, byte[] payload);
        ValueTask<bool> SendAsync(byte[] cmd, string message);
        ValueTask<Packet?> ReceiveAsync(CancellationToken cancellationToken);  // Nhận dữ liệu từ client
    }
}
