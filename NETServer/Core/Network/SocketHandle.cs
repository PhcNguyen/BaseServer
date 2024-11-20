using System.Net.Sockets;

using NETServer.Core.Network.Buffers;
using NETServer.Infrastructure.Services;
using NETServer.Core.Network.Packet;

namespace NETServer.Core.Network
{
    internal class SocketHandle(Socket socket, Guid id)
    {
        private readonly Guid _id = id;
        private readonly Socket _socket = socket;
        private readonly int _bufferDefaultSize = 128;
        private readonly MultiSizeBuffer _multiSizeBuffer = Singleton.GetInstance<MultiSizeBuffer>();
        private readonly PacketContainer _packetContainer = Singleton.GetInstance<PacketContainer>();
        
        public void HandleSocketError(SocketError socketError)
        {
            Console.WriteLine($"Socket Error: {socketError}");
            DisposeSocket();
        }

        public void DisposeSocket()
        {
            _socket?.Close();
        }

        public async Task StartReceiveAsync()
        {
            if (_socket == null) return;

            byte[] buffer = _multiSizeBuffer.RentBuffer(_bufferDefaultSize);
            var eventArgs = new SocketAsyncEventArgsN(_socket, _id, this);
            eventArgs.SetBuffer(buffer, 0, buffer.Length);
            eventArgs.Completed += OnReceiveCompleted;

            if (!_socket.ReceiveAsync(eventArgs))
            {
                await ProcessReceivedData(eventArgs);
            }
        }

        private async void OnReceiveCompleted(object? sender, SocketAsyncEventArgs e)
        {
            if (e is SocketAsyncEventArgsN args)
            {
                await ProcessReceivedData(args);
            }
            else
            {
                Console.WriteLine("EventArgs is not of type SocketAsyncEventArgsN.");
            }
        }

        public async Task ProcessReceivedData(SocketAsyncEventArgsN e)
        {
            if (e.Buffer == null)
            {
                Console.WriteLine("Buffer is null.");
                return;
            }

            byte[] buffer = e.Buffer;
            int bytesTransferred = e.BytesTransferred;

            Console.WriteLine($"Received {bytesTransferred} bytes for session {_id}.");

            var data = new byte[bytesTransferred];
            Array.Copy(buffer, data, bytesTransferred);

            ProcessPacket(data);

            await StartReceiveAsync();
        }

        private void ProcessPacket(byte[] data)
        {
            var packet = PacketExtensions.CreatePacket(_id, data: data);
            if (packet != null)
            {
                _packetContainer.AddPacket(_id, packet);
            }
        }

        public async Task SendDataAsync(byte[] data)
        {
            if (_socket == null) return;

            byte[] buffer = _multiSizeBuffer.RentBuffer(data.Length);
            Array.Copy(data, buffer, data.Length);
            var eventArgs = new SocketAsyncEventArgsN(_socket, _id, this);
            eventArgs.SetBuffer(buffer, 0, data.Length);
            eventArgs.Completed += OnSendCompleted;

            if (!_socket.SendAsync(eventArgs))
            {
                await ProcessSentData(eventArgs);
            }
        }

        private async void OnSendCompleted(object? sender, SocketAsyncEventArgs e)
        {
            if (e is SocketAsyncEventArgsN args)
            {
                await ProcessSentData(args);
            }
            else
            {
                Console.WriteLine("EventArgs is not of type SocketAsyncEventArgsN.");
            }
        }

        public async Task ProcessSentData(SocketAsyncEventArgsN e)
        {
            if (e.Buffer == null)
            {
                Console.WriteLine("Buffer is null.");
                return;
            }

            Console.WriteLine($"Sent {e.Count} bytes for session {_id}.");

            await StartReceiveAsync();
        }
    }
}