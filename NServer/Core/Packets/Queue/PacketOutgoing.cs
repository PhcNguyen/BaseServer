using System;

using Base.Core.Interfaces.Packets;

namespace Base.Core.Packets.Queue
{
    /// <summary>
    /// Hàng đợi gói tin dùng để xử lý các gói tin gửi.
    /// </summary>
    internal class PacketOutgoing : PacketQueue, IPacketOutgoing
    {
        public event Action? PacketAdded;

        public PacketOutgoing() : base() { }

        public void AddPacket(IPacket packet)
        {
            Enqueue(packet);

            // Kích hoạt sự kiện thông báo gói tin mới được thêm vào
            PacketAdded?.Invoke();
        }
    }
}
