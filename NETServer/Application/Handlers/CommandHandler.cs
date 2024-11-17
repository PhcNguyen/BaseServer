using NETServer.Infrastructure.Interfaces;
using System.Reflection;
using System.Threading.Tasks;
using NETServer.Network.Packets;

namespace NETServer.Application.Handlers
{
    internal class CommandHandler
    {
        private readonly Dictionary<Cmd, Func<IClientSession, byte[], CancellationToken, ValueTask>> _commandHandlers;

        public CommandHandler()
        {
            // Khởi tạo dictionary để lưu trữ các handler theo command
            _commandHandlers = [];
            RegisterHandlers();
        }

        // Đăng ký các phương thức handler có CommandAttribute
        private void RegisterHandlers()
        {
            // Tìm tất cả các method có CommandAttribute trong class
            var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var commandAttribute = method.GetCustomAttribute<CommandAttribute>();
                if (commandAttribute != null)
                {
                    // Tạo delegate cho handler từ method
                    var handler = (Func<IClientSession, byte[], CancellationToken, ValueTask>)method
                        .CreateDelegate(typeof(Func<IClientSession, byte[], CancellationToken, ValueTask>), this);

                    // Đăng ký handler với command tương ứng
                    _commandHandlers[commandAttribute.Command] = handler;
                }
            }
        }

        // Xử lý lệnh gửi từ client
        public async ValueTask HandleCommand(IClientSession session, Packet packet, CancellationToken cancellationToken)
        {
            if (packet.Command == null)
            {
                // Gửi thông báo lỗi nếu không tìm thấy Command hợp lệ
                await session.Transport.SendAsync((short)Cmd.ERROR, "Invalid command: Command is null or invalid.");
                return;
            }

            if (!_commandHandlers.TryGetValue((Cmd)packet.Command, out var handler))
            {
                // Gửi thông báo lỗi nếu không tìm thấy handler tương ứng
                await session.Transport.SendAsync((short)Cmd.ERROR, $"Unknown command: {(Cmd)packet.Command}");
                return;
            }

            try
            {
                // Gọi handler tương ứng với lệnh
                await handler(session, packet.Payload.ToArray(), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                await session.Transport.SendAsync((short)Cmd.ERROR, $"Command {(Cmd)packet.Command} was cancelled.");
            }
            catch (Exception ex)
            {
                await session.Transport.SendAsync((short)Cmd.ERROR, $"Error processing command {(Cmd)packet.Command}: {ex.Message}");
            }
        }

        // Handler cho PING
        [Command(Cmd.PING)]
        public static async ValueTask HandlePing(IClientSession session, byte[] payload)
        {
            await session.Transport.SendAsync((short)Cmd.PONG, "PONG");
        }

        // Handler cho GET_KEY
        [Command(Cmd.GET_KEY)]
        public static async ValueTask HandleGetKey(IClientSession session, byte[] payload)
        {
            string key = "123456"; // Lấy giá trị key
            await session.Transport.SendAsync((short)Cmd.GET_KEY, key); // Gửi key cho client
        }
    }
}