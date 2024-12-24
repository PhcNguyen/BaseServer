using NPServer.Common.Interfaces.Memory;
using NPServer.Common.Interfaces.Packets;
using NPServer.Common.Packets.Metadata;
using System;

namespace NPServer.Common.Packets;

/// <summary>
/// Lớp cơ sở cho tất cả các gói tin mạng.
/// </summary>
public partial class Packet : IPacket, IPoolable
{
    /// <summary>
    /// Id gói tin.
    /// </summary>
    public string? Id { get; private set; }

    /// <summary>
    ///Constructor mặc định.
    /// </summary>
    public Packet()
    { }

    /// <summary>
    /// Constructor để tạo Packet với các tham số tuỳ chọn.
    /// </summary>
    public Packet(PacketType type = PacketType.None, PacketFlags flags = PacketFlags.None, Enum? command = null, byte[]? payload = null)
    {
        Initialize(type, flags, ConvertCommand(command), payload);
    }

    /// <summary>
    /// Constructor thêm tham số cụ thể hơn.
    /// </summary>
    public Packet(PacketType type, PacketFlags flags, short command, byte[]? payload)
    {
        Initialize(type, flags, command, payload);
    }

    private static short ConvertCommand(object? command) => command switch
    {
        Enum enumCommand => Convert.ToInt16(enumCommand),
        short shortCommand => shortCommand,
        _ => 0
    };

    private void Initialize(PacketType type, PacketFlags flags, short command, byte[]? payload)
    {
        Type = type;
        Flags = flags;
        Cmd = command;
        PayloadData = payload?.Length > 0 ? new Memory<byte>(payload) : Memory<byte>.Empty;
    }

    /// <summary>
    /// Đặt ID cho gói tin.
    /// </summary>
    public void SetId(string id) => Id = id;

    /// <summary>
    /// Đặt lại gói tin về trạng thái ban đầu.
    /// </summary>
    public void ResetForPool()
    {
        Id = string.Empty;
        Flags = PacketFlags.None;
        Cmd = -1;
        PayloadData = Memory<byte>.Empty;
        _signature = [];
    }
}