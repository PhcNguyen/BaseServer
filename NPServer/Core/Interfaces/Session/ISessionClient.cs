using NPServer.Infrastructure.Services;

namespace NPServer.Core.Interfaces.Session;

public interface ISessionClient
{
    UniqueId Id { get; }
    ISessionNetwork Network { get; }

    byte[] Key { get; }
    bool IsConnected { get; }
    string IpAddress { get; }
    bool Authenticator { get; set; }

    void Connect();

    void Disconnect();

    void Dispose();

    void UpdateLastActivityTime();

    bool IsSessionTimedOut();

    bool IsSocketInvalid();
}