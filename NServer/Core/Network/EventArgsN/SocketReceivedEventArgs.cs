using System;

namespace NServer.Core.Network.EventArgsN
{
    internal class SocketReceivedEventArgs(byte[] data) : EventArgs
    {
        public byte[] Data { get; } = data;
    }
}
