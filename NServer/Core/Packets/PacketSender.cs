﻿using NServer.Core.Packets.Utils;
using System;

namespace NServer.Core.Packets
{
    /// <summary>
    /// Hàng đợi gói tin dùng để xử lý các gói tin gửi.
    /// </summary>
    internal class PacketSender : BasePacketContainer
    {
        public event Action? PacketAdded;

        public PacketSender() : base() { }

        public void AddPacket(Packet packet)
        {
            EnqueuePacket(packet);

            // Kích hoạt sự kiện thông báo gói tin mới được thêm vào
            PacketAdded?.Invoke();
        }
    }
}
