using NPServer.Core.Interfaces.Memory;
using NPServer.Core.Interfaces.Packets;
using NPServer.Core.Packets.Metadata;
using NPServer.Shared.Services;
using System;

namespace NPServer.Core.Packets;

/// <summary>
/// Lớp cơ sở cho tất cả các gói tin mạng.
/// </summary>
public partial class Packet : IPacket, IPoolable
{
    /// <summary>
    /// Id gói tin.
    /// </summary>
    public UniqueId Id { get; private set; }

    // Constructor mặc định
    public Packet()
    { }

    /// <summary>
    /// Constructor để tạo Packet với các tham số tuỳ chọn.
    /// </summary>
    public Packet(PacketType type = PacketType.NONE, PacketFlags flags = PacketFlags.NONE, Enum? command = null, byte[]? payload = null)
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
    public void SetId(UniqueId id) => Id = id;

    /// <summary>
    /// Đặt lại gói tin về trạng thái ban đầu.
    /// </summary>
    public void ResetForPool()
    {
        Id = UniqueId.Empty;
        Flags = PacketFlags.NONE;
        Cmd = -1;
        PayloadData = Memory<byte>.Empty;
        _signature = [];
    }
}