﻿using NServer.Core.Packets.Utils;
using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Services;
using System;

namespace NServer.Core.Packets
{
    /// <summary>
    /// Hàng đợi gói tin dùng để xử lý các gói tin nhận.
    /// </summary>
    internal class PacketReceiver : BasePacketContainer
    {
        public event Action? PacketAdded;

        public PacketReceiver() : base() { }

        public bool AddPacket(ID36 id, byte[]? packet)
        {
            try
            {
                if (packet == null)
                {
                    NLog.Instance.Warning("Packet");
                    return false;
                }
                if (PacketExtensions.IsValidPacket(packet))
                {
                    NLog.Instance.Warning("Packet Faild");
                    return false;
                }

                if (PacketExtensions.VerifyChecksum(packet)) {
                    NLog.Instance.Warning("Packet Faild 2");
                    return false;
                }

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
    }
}