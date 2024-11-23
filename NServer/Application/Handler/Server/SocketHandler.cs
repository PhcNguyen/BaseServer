using NServer.Interfaces.Core.Network;

namespace NServer.Application.Handler.Server
{
    internal class SocketHandler
    {
        [Command(Cmd.PING)]
        public static async Task Ping(ISession session, byte[] data)
        {
            await Task.Delay(0);
        }

        [Command(Cmd.CLOSE)]
        public static async Task Close(ISession session, byte[] data)
        {
            session.SocketAsync.Dispose();
            await session.Disconnect();
        }
    }
}
