using NETServer.Application.Handlers;

namespace NETServer.Infrastructure.Interfaces
{
    internal interface IDataTransmitter
    {
        bool IsEncrypted { get; }

        Task<bool> SendAsync(byte[] payload); // Gửi dữ liệu tới client
        Task<(Command command, byte[] data)> ReceiveAsync(CancellationToken cancellationToken);  // Nhận dữ liệu từ client
    }
}
