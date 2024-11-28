using System;
using Base.Core.Packets.Utils;
using Base.Core.Interfaces.Packets;
using Base.Infrastructure.Services;

namespace Base.Core.Packets.Queue
{
    /// <summary>
    /// Hàng đợi gói tin dùng để xử lý các gói tin nhận.
    /// </summary>
    internal class PacketIncoming : PacketQueue, IPacketIncoming
    {
        public event Action? PacketAdded;

        public PacketIncoming() : base() { }

        public bool AddPacket(UniqueId id, byte[]? packet)
        {
            try
            {
                if (packet == null) return false;

                Packet rpacket = PacketExtensions.FromByteArray(packet);
                rpacket.SetID(id);

                Enqueue(rpacket);

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
                if (packet == null) return false;

                Packet rpacket = PacketExtensions.FromByteArray(packet);

                Enqueue(rpacket);

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