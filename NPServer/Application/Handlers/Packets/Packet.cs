using NPServer.Core.Communication.Base;
using NPServer.Core.Communication.Metadata;
using NPServer.Core.Memory;
using NPServer.Models.Common;
using System;

namespace NPServer.Application.Handlers.Packets;

/// <summary>
/// Gói tin cơ bản, kế thừa từ AbstractPacket.
/// </summary>
public sealed class Packet : AbstractPacket, IPoolable
{
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Guests;

    // Constructor mặc định
    public Packet()
    { }

    /// <summary>
    /// Constructor để tạo Packet với các tham số tuỳ chọn.
    /// </summary>
    public Packet(byte? type = null, byte? flags = null, object? command = null, byte[]? payload = null)
    {
        short commandValue = GetCommandValue(command);
        Initialize(
            type.HasValue && Enum.IsDefined(typeof(PacketType), type.Value) ? (PacketType)type.Value : PacketType.NONE,
            flags.HasValue && Enum.IsDefined(typeof(PacketFlags), flags.Value) ? (PacketFlags)flags.Value : PacketFlags.NONE,
            commandValue,
            payload
        );
    }

    /// <summary>
    /// Constructor thêm tham số PacketType, PacketFlags, Enum command, byte[] payload.
    /// </summary>
    public Packet(PacketType type, PacketFlags flags, Enum command, byte[] payload)
    {
        Initialize(type, flags, Convert.ToInt16(command), payload);
    }

    // Phương thức để lấy giá trị command
    private static short GetCommandValue(object? command)
    {
        if (command is Enum enumCommand)
        {
            return Convert.ToInt16(enumCommand);
        }
        else if (command is short shortCommand)
        {
            return shortCommand;
        }
        return 0;
    }

    private void Initialize(PacketType type, PacketFlags flags, short command, byte[]? payload)
    {
        Type = type;
        Flags = flags;
        Cmd = command;

        if (payload != null && payload.Length + PacketMetadata.HEADERSIZE > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(payload), "The payload is too large.");
        }

        // Nếu payload không null và có dữ liệu, thì sử dụng Memory<byte>, nếu không thì dùng Memory<byte>.Empty
        PayloadData = payload?.Length > 0 ? new Memory<byte>(payload) : Memory<byte>.Empty;
    }
}