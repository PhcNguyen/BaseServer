using NPServer.Infrastructure.Time;
using System;
using System.Threading;

namespace NPServer.Infrastructure.Services
{
    /// <summary>
    /// Loại ID để phục vụ cho các mục đích khác nhau trong hệ thống.
    /// </summary>
    public enum IdType
    {
        /// <summary>
        /// ID chung không có mục đích cụ thể.
        /// </summary>
        Generic = 0,

        /// <summary>
        /// ID phiên bản khu vực (thời gian chạy).
        /// </summary>
        Region = 1,

        /// <summary>
        /// ID tài khoản hoặc người chơi trong cơ sở dữ liệu.
        /// </summary>
        Player = 2,

        /// <summary>
        /// ID kết nối khách hàng (thời gian chạy).
        /// </summary>
        Session = 3,

        /// <summary>
        /// ID phiên bản trò chơi (thời gian chạy).
        /// </summary>
        Game = 4,

        /// <summary>
        /// ID đại diện cho các thực thể (hình đại diện, vật phẩm, v.v.).
        /// </summary>
        Entity = 5,

        /// <summary>
        /// Giới hạn loại ID, không được vượt qua giá trị này.
        /// </summary>
        Limit = 1 << 4
    }

    /// <summary>
    /// GenId là bộ tạo ID 64-bit dựa trên cấu trúc Snowflake, cho phép tạo ID duy nhất
    /// một cách nhanh chóng và hiệu quả trong môi trường đa luồng.
    /// </summary>
    public sealed class GenId
    {
        // Dựa trên cấu trúc Snowflake:
        // - 4 bit: Loại ID.
        // - 12 bit: ID máy (tối đa 4096 máy).
        // - 32 bit: Unix timestamp tính bằng millisecond.
        // - 16 bit: Số thứ tự (sequence) của máy.

        private readonly IdType _type;
        private readonly ushort _machineId;
        private readonly Lock _lockObject = new();

        private int _sequenceNumber;
        private long _lastTimestamp = (long)Clock.UnixTime.TotalMilliseconds;

        /// <summary>
        /// Tạo một thể hiện mới của <see cref="GenId"/>.
        /// </summary>
        /// <param name="type">Loại ID để tạo.</param>
        /// <param name="machineId">ID của máy (tối đa 4095).</param>
        /// <exception cref="OverflowException">
        /// Ném lỗi nếu <paramref name="type"/> hoặc <paramref name="machineId"/> vượt quá giới hạn bit được định nghĩa.
        /// </exception>
        public GenId(IdType type, ushort machineId = 0)
        {
            if (type >= IdType.Limit)
                throw new OverflowException("Type exceeds 4 bits.");
            if (machineId >= 1 << 12)
                throw new OverflowException("MachineId exceeds 12 bits.");

            _type = type;
            _machineId = machineId;
        }

        /// <summary>
        /// Tạo một ID 64-bit duy nhất dựa trên cấu trúc Snowflake.
        /// </summary>
        /// <returns>ID 64-bit được tạo.</returns>
        public ulong Generate()
        {
            long timestamp = (long)Clock.UnixTime.TotalMilliseconds;
            int sequence;

            lock (_lockObject)
            {
                if (timestamp == _lastTimestamp)
                {
                    sequence = ++_sequenceNumber & 0xFFFF;
                    if (sequence == 0)
                        timestamp = WaitForNextMillis(_lastTimestamp);
                }
                else
                {
                    _sequenceNumber = 0;
                    sequence = 0;
                }

                _lastTimestamp = timestamp;
            }

            return AssembleId(timestamp, sequence);
        }

        /// <summary>
        /// Chờ cho đến khi đạt đến millisecond tiếp theo nếu thời gian hiện tại vẫn giống với thời gian trước đó.
        /// </summary>
        /// <param name="lastTimestamp">Thời gian trước đó.</param>
        /// <returns>Thời gian mới (millisecond).</returns>
        private static long WaitForNextMillis(long lastTimestamp)
        {
            long timestamp;
            do
            {
                timestamp = (long)Clock.UnixTime.TotalMilliseconds;
            } while (timestamp <= lastTimestamp);
            return timestamp;
        }

        /// <summary>
        /// Lắp ráp các thành phần để tạo thành ID cuối cùng.
        /// </summary>
        /// <param name="timestamp">Thời gian (millisecond).</param>
        /// <param name="sequence">Số thứ tự của máy.</param>
        /// <returns>ID 64-bit được tạo thành.</returns>
        private ulong AssembleId(long timestamp, int sequence)
        {
            ulong id = 0;
            id |= (ulong)_type << 60;
            id |= (ulong)_machineId << 48;
            id |= ((ulong)(timestamp & 0xFFFFFFFF)) << 16;
            id |= (ushort)sequence;
            return id;
        }

        /// <summary>
        /// Phân tích metadata từ một ID đã được tạo.
        /// </summary>
        /// <param name="id">ID cần phân tích.</param>
        /// <returns>Đối tượng <see cref="ParsedId"/> chứa thông tin chi tiết.</returns>
        public static ParsedId Parse(ulong id) => new(id);

        /// <summary>
        /// Cấu trúc lưu trữ metadata của một ID được phân tích.
        /// </summary>
        public readonly struct ParsedId(ulong id)
        {
            /// <summary>
            /// Loại ID.
            /// </summary>
            public IdType Type { get; } = (IdType)(id >> 60);

            /// <summary>
            /// ID của máy đã tạo ra ID này.
            /// </summary>
            public ushort MachineId { get; } = (ushort)(id >> 48 & 0xFFF);

            /// <summary>
            /// Thời gian tạo ID.
            /// </summary>
            public DateTime Timestamp { get; } = Clock.UnixTimeToDateTime(TimeSpan.FromMilliseconds(id >> 16 & 0xFFFFFFFF));

            /// <summary>
            /// Số thứ tự của máy tại thời điểm tạo ID.
            /// </summary>
            public ushort SequenceNumber { get; } = (ushort)(id & 0xFFFF);

            /// <summary>
            /// Trả về chuỗi mô tả metadata của ID.
            /// </summary>
            /// <returns>Chuỗi chứa thông tin chi tiết.</returns>
            public override string ToString()
            {
                return $"{Type} | Machine: 0x{MachineId:X} | {Timestamp} | Sequence: {SequenceNumber}";
            }
        }
    }
}
