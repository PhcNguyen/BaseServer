using System;

using NServer.Core.Packets.Utils;
using NServer.Core.Interfaces.Packets;
using NServer.Infrastructure.Services;

namespace NServer.Core.Packets
{
    /// <summary>
    /// Hàng đợi gói tin dùng để xử lý các gói tin nhận.
    /// </summary>
    internal class PacketReceiver : BasePacketContainer, IPacketReceiver
    {
        public event Action? PacketAdded;

        public PacketReceiver() : base() { }

        public bool AddPacket(UniqueId id, byte[]? packet)
        {
            try
            {
                if (packet == null
                    || PacketExtensions.IsValidPacket(packet)
                    || PacketExtensions.VerifyChecksum(packet)) return false;

                Packet rpacket = PacketExtensions.FromByteArray(packet);
                rpacket.SetID(id);

                EnqueuePacket(rpacket);

                // Kích hoạt sự kiện thông báo gói tin mới được thêm vào
                PacketAdded?.Invoke();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool AddPacket(byte[]? packet)
        {
            try
            {
                if (packet == null 
                    || PacketExtensions.IsValidPacket(packet)
                    || PacketExtensions.VerifyChecksum(packet)) return false;

                Packet rpacket = PacketExtensions.FromByteArray(packet);

                EnqueuePacket(rpacket);

                // Kích hoạt sự kiện thông báo gói tin mới được thêm vào
                PacketAdded?.Invoke();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}