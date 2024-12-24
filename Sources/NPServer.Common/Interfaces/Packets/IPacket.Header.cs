using NPServer.Common.Packets.Metadata;

namespace NPServer.Common.Interfaces.Packets;

public partial interface IPacket
{
    /// <summary>
    /// Lấy hoặc đặt kiểu của gói tin.
    /// </summary>
    PacketType Type { get; }

    /// <summary>
    /// Lấy hoặc đặt các cờ trạng thái của gói tin.
    /// </summary>
    PacketFlags Flags { get; }

    /// <summary>
    /// Lấy hoặc đặt mã lệnh của gói tin.
    /// </summary>
    short Cmd { get; }

    /// <summary>
    /// Đặt kiểu cho gói tin.
    /// </summary>
    /// <param name="type">Kiểu mới cho gói tin.</param>
    void SetType(PacketType type);

    /// <summary>
    /// Bật một cờ trạng thái cho gói tin.
    /// </summary>
    /// <param name="flag">Cờ trạng thái cần bật.</param>
    void EnableFlag(PacketFlags flag);

    /// <summary>
    /// Tắt một cờ trạng thái cho gói tin.
    /// </summary>
    /// <param name="flag">Cờ trạng thái cần tắt.</param>
    void DisableFlag(PacketFlags flag);

    /// <summary>
    /// Kiểm tra xem gói tin có cờ trạng thái nhất định hay không.
    /// </summary>
    /// <param name="flag">Cờ trạng thái cần kiểm tra.</param>
    /// <returns>Trả về `true` nếu gói tin có cờ trạng thái, ngược lại `false`.</returns>
    bool HasFlag(PacketFlags flag);

    /// <summary>
    /// Đặt mã lệnh cho gói tin từ giá trị ngắn.
    /// </summary>
    /// <param name="command">Mã lệnh mới.</param>
    void SetCmd(short command);

    /// <summary>
    /// Đặt mã lệnh cho gói tin từ một Enum.
    /// </summary>
    /// <param name="command">Giá trị Enum cho mã lệnh.</param>
    void SetCmd(System.Enum command);
}