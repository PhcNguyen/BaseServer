namespace NServer.Core.Interfaces.Session;

public interface ISessionConnection
{
    bool IsConnected { get; }

    string IpAddress { get; }

    void UpdateLastActivity();

    void SetTimeout(System.TimeSpan timeout);

    bool IsTimedOut();

    void Dispose();
}
