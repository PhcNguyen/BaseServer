using NServer.Core.Interfaces.Packets;
using NServer.Core.Packets;

namespace NServer.Application.Handlers.Packets
{
    internal static class PacketUtils
    {
        public static readonly IPacket EmptyPacket = new Packet(0, 0, 0, []);

        public static IPacket Response(Command command, string message)
        {
            Packet packet = new();
            packet.SetCmd(command);
            packet.SetPayload(message);
            return packet;
        }
    }
}