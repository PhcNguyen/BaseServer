using NPServer.Infrastructure.Collections;

namespace NPServer.Application.Handlers.Packets.Queue;

/// <summary>
/// Hàng đợi các gói tin (Packet) cho các thao tác liên quan đến xử lý gói tin trong hệ thống.
/// </summary>
public class PacketQueue : CustomQueues<Packet>
{
    /// <summary>
    /// Khởi tạo một hàng đợi gói tin.
    /// </summary>
    public PacketQueue() : base()
    {
    }
}
