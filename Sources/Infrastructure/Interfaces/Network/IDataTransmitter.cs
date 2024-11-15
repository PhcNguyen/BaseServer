using NETServer.Application.Enums;

namespace NETServer.Infrastructure.Interfaces
{
    internal interface IDataTransmitter
    {
        bool IsEncrypted { get; }

        void Create(Stream stream, byte[] sessionKey);
        Task<bool> SendAsync(byte[] payload); // Gửi dữ liệu tới client
        Task<(Cmd command, byte[] data)> ReceiveAsync(CancellationToken cancellationToken);  // Nhận dữ liệu từ client
    }
}
