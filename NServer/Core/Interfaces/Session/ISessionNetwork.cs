using System;

namespace NPServer.Core.Interfaces.Session;

public interface ISessionNetwork
{
    event Action<byte[]>? DataReceived;

    bool Send(byte[] data);

    bool Send(string data);
}