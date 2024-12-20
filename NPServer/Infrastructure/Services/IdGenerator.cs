using NPServer.Infrastructure.Services.Time;
using System;
using System.Threading;

namespace NPServer.Infrastructure.Services;

public enum IdType
{
    Generic = 0,
    Region = 1,      // [Chạy thời gian] ID phiên bản khu vực
    Player = 2,      // [Cơ sở dữ liệu] Tài khoản và các thực thể người chơi tồn tại đại diện cho những tài khoản đó trong trò chơi
    Session = 3,     // [Chạy thời gian] ID kết nối khách hàng
    Game = 4,        // [Chạy thời gian] ID phiên bản trò chơi
    Entity = 5,      // [Cơ sở dữ liệu] Các thực thể tồn tại (hình đại diện, vật phẩm, v.v.)
    Limit = 1 << 4   // 16
}

/// <summary>
/// Tạo các id 64-bit giống như snowflake cho nhiều mục đích khác nhau.
/// </summary>
public sealed class IdGenerator
{
    // Dựa trên id snowflake (xem tại đây: https://en.wikipedia.org/wiki/Snowflake_ID)
    // Cấu trúc hiện tại:
    //  4 bit - loại
    // 12 bit - id máy (để tạo id của cùng một loại song song, tối đa 4096 phiên bản cùng một lúc)
    // 32 bit - unix timestamp tính bằng giây
    // 16 bit - số thứ tự của máy (để tránh trùng lặp nếu nhiều id được tạo trong cùng một giây)

    private readonly ReaderWriterLockSlim _lock = new();
    private readonly IdType _type;
    private readonly ushort _machineId;
    private int _machineSequenceNumber = 0;

    /// <summary>
    /// Xây dựng một phiên bản mới của <see cref="IdGenerator"/>. Id máy phải < 4096.
    /// </summary>
    public IdGenerator(IdType type, ushort machineId = 0)
    {
        if (type >= IdType.Limit) throw new OverflowException("Type exceeds 4 bits.");
        if (machineId >= 1 << 12) throw new OverflowException("MachineId exceeds 12 bits.");

        _type = type;
        _machineId = machineId;
    }

    /// <summary>
    /// Tạo một id 64-bit mới.
    /// </summary>
    public ulong Generate()
    {
        _lock.EnterWriteLock();
        try
        {
            ulong id = 0;
            id |= (ulong)_type << 60;
            id |= (ulong)_machineId << 48;
            id |= ((ulong)Clock.UnixTime.TotalSeconds & 0xFFFFFFFF) << 16;
            id |= (ushort)Interlocked.Increment(ref _machineSequenceNumber);
            return id;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Phân tích metadata từ id được tạo.
    /// </summary>
    public static ParsedId Parse(ulong id) => new(id);

    /// <summary>
    /// Id đã được phân tích bởi <see cref="IdGenerator"/>.
    /// </summary>
    public readonly struct ParsedId(ulong id)
    {
        public IdType Type { get; } = (IdType)(id >> 60);
        public ushort MachineId { get; } = (ushort)(id >> 48 & 0xFFF);
        public DateTime Timestamp { get; } = Clock.UnixTimeToDateTime(TimeSpan.FromSeconds(id >> 16 & 0xFFFFFFFF));
        public ushort MachineSequenceNumber { get; } = (ushort)(id & 0xFFFF);

        public override string ToString()
        {
            return $"{Type} | 0x{MachineId:X} | {Timestamp} | {MachineSequenceNumber}";
        }
    }
}