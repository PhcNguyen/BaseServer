using NETServer.Infrastructure.Security;

namespace NETServer.Application.Handlers
{
    internal class CommandHandler
    {
        private readonly Dictionary<Command, Func<byte[], Task<byte[]>>> _commandHandlers;
        private readonly RsaCipher _rsaCipher;

        public CommandHandler()
        {
            // Khởi tạo dictionary ánh xạ các command với các phương thức xử lý
            _commandHandlers = new Dictionary<Command, Func<byte[], Task<byte[]>>>
            {
                { Command.PING, HandlePing },
                { Command.GET_KEY, HandleGetKey }
            };

            _rsaCipher = new RsaCipher();
        }

        // Phương thức chính để xử lý command và trả về dữ liệu
        public async Task<byte[]> HandleCommand(Command command, byte[] data)
        {
            if (_commandHandlers.TryGetValue(command, out var handler))
            {
                return await handler(data);
            }
            else
            {
                // Xử lý trường hợp không có handler cho command này
                Console.WriteLine($"No handler found for command {command}");
                return new byte[] { };
            }
        }

        // Ví dụ các phương thức xử lý cho từng command và trả về byte[]
        private async Task<byte[]> HandlePing(byte[] data)
        {

            await Task.CompletedTask;
            return new byte[] {}; 
        }

        private async Task<byte[]> HandleGetKey(byte[] data)
        {
            var key = RsaCipher.ImportPublicKey(data);


            await Task.CompletedTask;
            return new byte[] { 0x04, 0x05, 0x06 }; // trả về dữ liệu sau khi xử lý
        }
    }
}
