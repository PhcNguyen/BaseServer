using NETServer.Application.Handlers;

namespace NETServer.Infrastructure.Interfaces
{
    internal interface IDataTransmitter
    {
        Task<bool> Send(byte[] payload); // Gửi dữ liệu tới client
        Task<(Command command, byte[] data)> Receive(CancellationToken cancellationToken);  // Nhận dữ liệu từ client
    }
}
