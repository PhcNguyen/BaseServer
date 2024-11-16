using NETServer.Infrastructure.Interfaces;
using NETServer.Infrastructure.Logging;
using NETServer.Application.Enums;
using NETServer.Application.Network.Transport;

namespace NETServer.Application.Handlers
{
    internal class CommandProcessor
    {
        private readonly Dictionary<Cmd, Func<IClientSession, byte[], CancellationToken, Task>> _commandHandlers;

        public CommandProcessor()
        {
            _commandHandlers = new Dictionary<Cmd, Func<IClientSession, byte[], CancellationToken, Task>>
            {
                { Cmd.PING, HandlePing },
                { Cmd.GET_KEY, HandleGetKey }
            };
        }

        public async Task HandleCommand(IClientSession session, Packet packet, CancellationToken cancellationToken)
        {
            if (packet.Command is null) return;

            if (!_commandHandlers.TryGetValue((Cmd)packet.Command[0], out var handler))
            {
                await UnknownCommand(session, (Cmd)packet.Command[0]);
                return;
            }

            try
            {
                await handler(session, packet.Payload, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                NLog.Warning($"Command {packet.Command} was cancelled.");
                await session.Transport.SendAsync("Command was cancelled.");
            }
            catch (Exception ex)
            {
                NLog.Error($"Error handling command {packet.Command}: {ex.Message}\n{ex.StackTrace}");
                await session.Transport.SendAsync("Error processing command");
            }
        }

        private static async Task UnknownCommand(IClientSession session, Cmd command)
        {
            if (session.Transport != null)
            {
                await session.Transport.SendAsync($"Unknown command: {command}");
                return;
            }
            throw new InvalidOperationException("DataTransport is null. The session is not properly initialized.");
        }

        private async Task HandlePing(IClientSession session, byte[] playload, CancellationToken cancellationToken)
        {
            // Xử lý lệnh PING, có thể cần phản hồi ngược lại cho client
            await session.Transport.SendAsync("PONG");
        }

        private async Task HandleGetKey(IClientSession session, byte[] playload, CancellationToken cancellationToken)
        {
            // Kiểm tra CancellationToken trước khi thực hiện công việc nặng
            cancellationToken.ThrowIfCancellationRequested();

            // Giả sử đây là key tạm thời
            string key = "123456";

            // Gửi phản hồi cho client
            await session.Transport.SendAsync(key);
        }
    }
}
