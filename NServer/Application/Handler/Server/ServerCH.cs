using System.Threading.Tasks;
using NServer.Core.Interfaces.Session;

namespace NServer.Application.Handler.Server
{
    internal class ServerCH
    {
        [Command(Cmd.PING)]
        public static async Task Ping(ISessionClient session, byte[] data)
        {
            await Task.Delay(0);
        }

        [Command(Cmd.CLOSE)]
        public static async Task Close(ISessionClient session, byte[] data)
        {
            await session.Disconnect();
        }
    }
}
