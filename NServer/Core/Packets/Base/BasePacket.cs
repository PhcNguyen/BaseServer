using NServer.Core.Services;
using System;

namespace NServer.Core.Packets.Base;

/// <summary>
/// Lớp cơ sở cho tất cả các gói tin mạng.
/// </summary>
public abstract partial class BasePacket
{
    /// <summary>
    /// Id gói tin.
    /// </summary>
    public UniqueId Id { get; protected set; }

    /// <summary>
    /// Tổng chiều dài của gói tin, bao gồm header và payload.
    /// </summary>
    public int Length => _headerSize + _payload.Length;

    /// <summary>
    /// Phương thức để thêm ID
    /// </summary>
    public void SetId(UniqueId id) => Id = id;

    /// <summary>
    /// Phương thức để đặt lại gói tin về trạng thái ban đầu.
    /// </summary>
    public void Reset()
    {
        Id = UniqueId.Empty;
        Flags = Enums.PacketFlags.NONE;
        Cmd = 0;
        Payload = Memory<byte>.Empty;
    }
}