using System;
using NServer.Core.Interfaces.Packets;

namespace NServer.Core.Packets.Queue
{
    /// <summary>
    /// Hàng đợi gói tin dùng để xử lý các gói tin nhận.
    /// </summary>
    internal class PacketIncoming : PacketQueue, IPacketIncoming
    {
        public event Action? PacketAdded;

        public PacketIncoming() : base() { }

        public bool AddPacket(IPacket packet)
        {
            try
            {
                Enqueue(packet);

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