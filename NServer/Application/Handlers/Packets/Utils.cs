using NServer.Core.Packets;

namespace NServer.Application.Handlers.Packets
{
    internal class Utils
    {
        public static readonly Packet EmptyPacket = new(0, 0, 0, []);

        public static Packet Response(Cmd command, string message)
        {
            var packet = new Packet();
            packet.SetCmd((short)command);
            packet.SetPayload(message);
            return packet;
        }


    }
}
