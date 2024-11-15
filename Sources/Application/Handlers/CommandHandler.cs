using NETServer.Infrastructure.Interfaces;
using NETServer.Application.Helper;
using NETServer.Infrastructure.Logging;  

namespace NETServer.Application.Handlers
{
    internal class CommandHandler
    {
        private readonly Dictionary<Command, Func<IClientSession, byte[], Task>> _commandHandlers;

        public CommandHandler()
        {
            _commandHandlers = new Dictionary<Command, Func<IClientSession, byte[], Task>>
            {
                { Command.PING, HandlePing },
                { Command.GET_KEY, HandleGetKey }
            };
        }

        public async Task HandleCommand(IClientSession session, Command command, byte[] data)
        {
            // Kiểm tra lệnh có hợp lệ hay không
            if (data == null || data.Length == 0)
            {
                await session.DataTransport.SendAsync(ByteHelper.ToBytes("Invalid data received"));
                return;
            }

            if (!_commandHandlers.TryGetValue(command, out var handler))
            {
                // Gửi thông báo lỗi khi không tìm thấy handler cho lệnh
                await HandleUnknownCommand(session, command);
                return;
            }

            try
            {
                // Thực thi lệnh đã tìm thấy
                await handler(session, data);
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi trong quá trình xử lý lệnh
                NLog.Error($"Error handling command {command}: {ex.Message}\n{ex.StackTrace}");
                await session.DataTransport.SendAsync(ByteHelper.ToBytes("Error processing command"));
            }
        }

        private async Task HandlePing(IClientSession session, byte[] data)
        {
            // Xử lý lệnh PING, có thể cần phản hồi ngược lại cho client
            await session.DataTransport.SendAsync(ByteHelper.ToBytes("PONG"));
        }

        private async Task HandleGetKey(IClientSession session, byte[] data)
        {
            // Xử lý lệnh GET_KEY, có thể cần trả về một giá trị
            string key = "123456"; // Giả sử đây là key tạm thời
            await session.DataTransport.SendAsync(ByteHelper.ToBytes(key));
        }

        private static async Task HandleUnknownCommand(IClientSession session, Command command)
        {
            if (session.DataTransport != null)
            {
                await session.DataTransport.SendAsync(ByteHelper.ToBytes($"Unknown command: {command}"));
                return;
            }
            throw new InvalidOperationException("DataTransport is null. The session is not properly initialized.");
        }
    }
}
