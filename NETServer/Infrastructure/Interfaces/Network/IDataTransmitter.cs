namespace NETServer.Infrastructure.Interfaces
{
    internal interface IDataTransmitter
    {
        bool IsEncrypted { get; }

        ValueTask<bool> SendAsync(short command, byte[] payload);
        ValueTask<bool> SendAsync(short command, string message);
        ValueTask<IPacket?> ReceiveAsync(CancellationToken cancellationToken);  // Nhận dữ liệu từ client
    }
}
