using NETServer.Infrastructure.Interfaces;
using NETServer.Application.Helper;
using NETServer.Application.Network;

namespace NETServer.Application.Handlers
{
    internal class CommandHandler
    {
        private readonly Dictionary<Command, Func<ClientSession, byte[], Task>> _commandHandlers;

        public CommandHandler()
        {
            _commandHandlers = new Dictionary<Command, Func<ClientSession, byte[], Task>>
            {
                { Command.PING, HandlePing },
                { Command.GET_KEY, HandleGetKey }
            };
        }

        // Phương thức chính để xử lý command
        public async Task HandleCommand(IClientSession session, Command command, byte[] data)
        {
            if (!_commandHandlers.TryGetValue(command, out var handler))
            {
                if (session.DataTransport != null)
                {
                    await session.DataTransport.SendAsync(ByteHelper.ToBytes("No handler found for command"));
                }
                else
                {
                    throw new InvalidOperationException("DataTransport is null. The session is not properly initialized.");
                }
            }
            else
            {
                try
                {
                    await handler((ClientSession)session, data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling command {command}: {ex.Message}");
                }
            }
        }

        private async Task HandlePing(IClientSession session, byte[] data)
        {
            // Xử lý ping, thực hiện các tác vụ cần thiết mà không cần trả về dữ liệu
            Console.WriteLine("Ping handled");
            await Task.CompletedTask;
        }

        private async Task HandleGetKey(IClientSession session, byte[] data)
        {
            // Xử lý GET_KEY, thực hiện các tác vụ cần thiết mà không cần trả về dữ liệu
            Console.WriteLine("GetKey handled");
            await Task.CompletedTask;
        }
    }
}