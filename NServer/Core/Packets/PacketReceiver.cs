using NServer.Core.Packets.Utils;
using NServer.Infrastructure.Services;

namespace NServer.Core.Packets
{
    /// <summary>
    /// Hàng đợi gói tin dùng để xử lý các gói tin nhận.
    /// </summary>
    internal class PacketReceiver : BasePacketContainer
    {
        public PacketReceiver() : base() { }

        public bool AddPacket(ID36 id, byte[]? packet)
        {
            try
            {
                if (packet == null
                    || PacketExtensions.IsValidPacket(packet)
                    || PacketExtensions.VerifyChecksum(packet))
                {
                    return false;
                }

                Packet rpacket = PacketExtensions.FromByteArray(packet);
                rpacket.SetID(id);

                EnqueuePacket(rpacket);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}