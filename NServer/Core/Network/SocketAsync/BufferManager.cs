using NServer.Core.Network.Buffers;
using NServer.Infrastructure.Services;

using System.Net.Sockets;

namespace NServer.Core.Network.SocketAsync
{
    internal class BufferManager
    {
        private readonly MultiSizeBuffer _multiSizeBuffer;
        private readonly int _bufferDefaultSize = 256;

        public BufferManager()
        {
            _multiSizeBuffer = Singleton.GetInstance<MultiSizeBuffer>();
        }

        public byte[] GetReceiveBuffer(SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs.UserToken is DisposableBuffer buffer)
            {
                return buffer.Buffer;
            }

            var newBuffer = new DisposableBuffer(_multiSizeBuffer.RentBuffer(_bufferDefaultSize), _multiSizeBuffer);
            eventArgs.UserToken = newBuffer;
            return newBuffer.Buffer;
        }

        public byte[] GetSendBuffer(SocketAsyncEventArgs eventArgs, int length)
        {
            if (eventArgs.UserToken is DisposableBuffer buffer && buffer.Buffer.Length >= length)
            {
                return buffer.Buffer;
            }

            ReleaseBuffer(eventArgs);
            var newBuffer = new DisposableBuffer(_multiSizeBuffer.RentBuffer(length), _multiSizeBuffer);
            eventArgs.UserToken = newBuffer;
            return newBuffer.Buffer;
        }

        public static void ReleaseBuffer(SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs.UserToken is DisposableBuffer buffer)
            {
                buffer.Dispose();
                eventArgs.UserToken = null;
            }
        }
    }

    internal class DisposableBuffer(byte[] buffer, MultiSizeBuffer multiSizeBuffer) : IDisposable
    {
        public byte[] Buffer { get; } = buffer ?? throw new ArgumentNullException(nameof(buffer));

        private readonly MultiSizeBuffer _multiSizeBuffer = multiSizeBuffer ?? throw new ArgumentNullException(nameof(multiSizeBuffer));
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _multiSizeBuffer.ReturnBuffer(Buffer);
            _disposed = true;
        }
    }
}