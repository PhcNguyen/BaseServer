using System;

namespace NPServer.Core.Network.IO;

public sealed class SocketReceivedEventArgs(byte[] data) : EventArgs
{
    public byte[] Data { get; } = data;
}