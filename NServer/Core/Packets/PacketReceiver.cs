using NServer.Core.Packets.Utils;

namespace NServer.Core.Packets
{
    /// <summary>
    /// Hàng đợi gói tin dùng để xử lý các gói tin nhận.
    /// </summary>
    internal class PacketReceiver : BasePacketContainer
    {
        public PacketReceiver() : base() { }

        public bool AddPacket(byte[]? packet)
        {
            try
            {
                if (packet == null
                    || PacketExtensions.IsValidPacket(packet)
                    || PacketExtensions.VerifyChecksum(packet))
                {
                    return false;
                }
                    

                EnqueuePacket(PacketExtensions.FromByteArray(packet));

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}