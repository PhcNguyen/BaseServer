using NPServer.Core.Interfaces.Packets;
using NPServer.Core.Packets.Metadata;
using System;

namespace NPServer.Core.Packets;

public partial class Packet : IPacket
{
    private const int _headerSize = PacketMetadata.HEADERSIZE;

    /// <summary>
    /// Type để xác định loại gói tin.
    /// </summary>
    public PacketType Type { get; private set; } = PacketType.NONE;

    /// <summary>
    /// Cờ trạng thái của gói tin.
    /// </summary>
    public PacketFlags Flags { get; private set; } = PacketFlags.NONE;

    /// <summary>
    /// Command để xác định loại gói tin.
    /// </summary>
    public short Cmd { get; private set; } = 0;

    /// <summary>
    /// Phương thức để thiết lập loại gói tin.
    /// </summary>
    public void SetType(PacketType type) => Type = type;

    /// <summary>
    /// Phương thức để thêm cờ trạng thái.
    /// </summary>
    public void EnableFlag(PacketFlags flag)
    {
        if (!HasFlag(flag))
            Flags |= flag;
    }

    /// <summary>
    /// Phương thức để loại bỏ cờ trạng thái.
    /// </summary>
    public void DisableFlag(PacketFlags flag)
    {
        if (HasFlag(flag))
            Flags &= ~flag;
    }

    /// <summary>
    /// Kiểm tra xem flag có tồn tại hay không.
    /// </summary>
    public bool HasFlag(PacketFlags flag) => (Flags & flag) == flag;

    /// <summary>
    /// Thiết lập Command mới cho gói tin.
    /// </summary>
    public void SetCmd(short cmd) => Cmd = cmd;

    /// <summary>
    /// Thiết lập giá trị lệnh từ một đối tượng enum bất kỳ.
    /// </summary>
    /// <param name="command">Đối tượng enum cần thiết lập.</param>
    public void SetCmd(Enum command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (Enum.IsDefined(command.GetType(), command))
            Cmd = Convert.ToInt16(command);
        else
            throw new ArgumentException("The provided enum value is invalid.", nameof(command));
    }

    public override string ToString()
    {
        return $"Packet: Cmd={Cmd}, Type={Type}, Flags={Flags}";
    }
}