using NPServer.Core.Interfaces.Packets;
using NPServer.Infrastructure.Collections;

namespace NPServer.Core.Packets.Queue;

/// <summary>
/// Hàng đợi các gói tin (Packet) cho các thao tác liên quan đến xử lý gói tin trong hệ thống.
/// </summary>
public class PacketQueue : CustomQueues<IPacket>
{
    /// <summary>
    /// Khởi tạo một hàng đợi gói tin.
    /// </summary>
    public PacketQueue() : base()
    {
    }
}