using System.Net.Sockets;

namespace NETServer.Core.Network.Buffers
{
    internal class SocketAsyncEventArgsN : SocketAsyncEventArgs
    {
        private readonly Guid _id;
        private readonly SocketHandle _socketHandler;

        public SocketAsyncEventArgsN(Socket socket, Guid id, SocketHandle socketHandler)
        {
            _id = id;
            AcceptSocket = socket ?? throw new ArgumentNullException(nameof(socket));
            _socketHandler = socketHandler;
            Completed += OnCompleted!;
        }

        private void OnCompleted(object? sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Console.WriteLine($"Operation completed successfully for session {_id}.");
                _ = _socketHandler.ProcessReceivedData(this);
            }
            else
            {
                _socketHandler.HandleSocketError(e.SocketError);
            }
        }
    }
}
