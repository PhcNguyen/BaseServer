using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NETServer.Application.Handlers;

internal class CommandHandler
{
    private readonly Dictionary<Command, Func<byte[], Task<byte[]>>> _commandHandlers;

    public CommandHandler()
    {
        // Khởi tạo dictionary ánh xạ các command với các phương thức xử lý
        _commandHandlers = new Dictionary<Command, Func<byte[], Task<byte[]>>>
        {
            { Command.SET_KEY, HandleSetKey },
            { Command.GET_KEY, HandleGetKey }
        };
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
    private async Task<byte[]> HandleSetKey(byte[] data)
    {
        Console.WriteLine("Handling SET_KEY command");
        // Thêm logic xử lý cho SET_KEY ở đây, ví dụ trả về một byte array
        await Task.CompletedTask;
        return new byte[] { 0x01, 0x02, 0x03 }; // trả về dữ liệu sau khi xử lý
    }

    private async Task<byte[]> HandleGetKey(byte[] data)
    {
        Console.WriteLine("Handling GET_KEY command");
        // Thêm logic xử lý cho GET_KEY ở đây, ví dụ trả về một byte array
        await Task.CompletedTask;
        return new byte[] { 0x04, 0x05, 0x06 }; // trả về dữ liệu sau khi xử lý
    }
}
