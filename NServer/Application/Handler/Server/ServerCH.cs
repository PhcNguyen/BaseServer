using System.Threading.Tasks;
using NServer.Core.Interfaces.Session;
using NServer.Core.Packets;

namespace NServer.Application.Handler.Server
{
    internal class ServerCH
    {
        public static readonly Packet EmptyPacket = new(0, 0, []);

        [Command(Cmd.PING)]
        public static async Task<Packet> Ping(byte[] data)
        {
            await Task.Delay(0);
            return await Task.FromResult<Packet>(EmptyPacket);
        }

        [Command(Cmd.CLOSE)]
        public static async Task<Packet> Close(byte[] data)
        {
            return await Task.FromResult<Packet>(EmptyPacket);
        }
    }
}
