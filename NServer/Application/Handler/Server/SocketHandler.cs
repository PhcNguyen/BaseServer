using NServer.Interfaces.Core.Network;

namespace NServer.Application.Handler.Server
{
    internal class SocketHandler
    {
        [Command(Cmd.CLOSE)]
        public static async Task CloseConnection(ISession session, byte[] data)
        {
            await session.Disconnect();
        }
    }
}
