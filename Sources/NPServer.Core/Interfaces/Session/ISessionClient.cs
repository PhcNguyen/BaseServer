using NPServer.Shared.Services;
using NPServer.Models.Common;

namespace NPServer.Core.Interfaces.Session;

public interface ISessionClient
{
    UniqueId Id { get; }
    ISessionNetwork Network { get; }

    byte[] Key { get; }
    bool IsConnected { get; }
    string IpAddress { get; }
    AccessLevel Role { get; }

    void Connect();

    void Disconnect();

    void Dispose();

    void UpdateLastActivityTime();

    bool IsSessionTimedOut();

    bool IsSocketInvalid();
}