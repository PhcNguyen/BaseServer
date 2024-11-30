using NServer.Core.Packets;
using NServer.Core.Interfaces.Packets;

namespace NServer.Application.Handlers.Packets
{
    internal class PacketUtils
    {
        public static readonly IPacket EmptyPacket = new Packet(0, 0, 0, []);

        public static IPacket Response(Cmd command, string message)
        {
            Packet packet = new();
            packet.SetCmd(command);
            packet.SetPayload(message);
            return packet;
        }
    }
}
