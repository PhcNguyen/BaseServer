using System;

namespace NPServer.Core.Network.IO;

public partial class SocketReceivedEventArgs(byte[] data) : EventArgs
{
    public byte[] Data { get; } = data;
}