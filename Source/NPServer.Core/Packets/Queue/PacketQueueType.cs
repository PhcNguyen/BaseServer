namespace NPServer.Core.Packets.Queue;

/// <summary>
/// Enum đại diện cho các loại hàng đợi gói tin khác nhau.
/// </summary>
public enum PacketQueueType
{
    /// <summary>
    /// Hàng đợi dành cho gói tin từ máy chủ.
    /// </summary>
    Server,

    /// <summary>
    /// Hàng đợi dành cho gói tin vào hệ thống.
    /// </summary>
    Incoming,

    /// <summary>
    /// Hàng đợi dành cho gói tin đi ra từ hệ thống.
    /// </summary>
    Outgoing
}