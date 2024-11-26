using System.Threading.Tasks;
using NServer.Core.Packets;

namespace NServer.Application.Handler.Client
{
    internal class Utils
    {
        public static readonly Packet EmptyPacket = new(0, 0, []);

        public static Packet Response(Cmd command, string message)
        {
            var packet = new Packet();
            packet.SetCommand((short)command);
            packet.SetPayload(message);
            return packet;
        }

        
    }
}
