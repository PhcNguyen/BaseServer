using NPServer.Core.Packets.Metadata;
using System;

namespace NPServer.Core.Packets.Base;

public partial class AbstractPacket
{
    private const int _headerSize = PacketMetadata.HEADERSIZE;

    /// <summary>
    /// Type để xác định loại gói tin.
    /// </summary>
    public PacketType Type { get; protected set; } = PacketType.NONE;

    /// <summary>
    /// Cờ trạng thái của gói tin.
    /// </summary>
    public PacketFlags Flags { get; protected set; } = PacketFlags.NONE;

    /// <summary>
    /// Command để xác định loại gói tin.
    /// </summary>
    public short Cmd { get; protected set; } = 0;

    /// <summary>
    /// Phương thức để thêm cờ Type
    /// </summary>
    public void SetType(PacketType type) => Type = type;

    /// <summary>
    /// Phương thức để thêm cờ trạng thái
    /// </summary>
    public void AddFlag(PacketFlags flag) => Flags |= flag;

    /// <summary>
    /// Phương thức để loại bỏ cờ trạng thái
    /// </summary>
    public void RemoveFlag(PacketFlags flag) => Flags &= ~flag;

    /// <summary>
    /// Kiểm tra xem flag có tồn tại hay không.
    /// </summary>
    public bool HasFlag(PacketFlags flag) => Flags.HasFlag(flag);

    /// <summary>
    /// Set Command mới cho gói tin.
    /// </summary>
    public void SetCmd(short cmd) => Cmd = cmd;

    /// <summary>
    /// Thiết lập giá trị lệnh từ một đối tượng enum bất kỳ.
    /// </summary>
    /// <param name="command">Đối tượng enum cần thiết lập.</param>
    public void SetCmd(object command) =>
        Cmd = command is System.Enum enumCommand ? Convert.ToInt16(enumCommand) : (short)0;
}