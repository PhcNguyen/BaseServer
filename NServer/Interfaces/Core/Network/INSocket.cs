using System;

namespace NServer.Interfaces.Core.Network
{
    internal interface INSocket : IDisposable
    {
        bool Disposed { get; }

        new void Dispose();
    }
}
