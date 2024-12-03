using System.Collections.Generic;

namespace NServer.Infrastructure.Configuration
{
    /// <summary>
    /// Lớp cấu hình bộ đệm, cung cấp các cấu hình liên quan đến việc phân bổ bộ đệm.
    /// </summary>
    internal static class BufferConfig
    {
        /// <summary>
        /// Tổng số bộ đệm. Hệ thống sẽ tự động tăng số lượng này khi thiếu.
        /// </summary>
        public static readonly int TotalBuffers = 1000;

        /// <summary>
        /// Bảng phân bổ bộ đệm theo kích thước và tỷ lệ.
        /// Kích thước được biểu diễn bằng byte và tỷ lệ được biểu diễn dưới dạng phần trăm.
        /// </summary>
        public static readonly (int BufferSize, double Allocation)[] BufferAllocations =
        [
            (256  , 0.40),
            (512  , 0.25),
            (1024 , 0.15),
            (2048 , 0.10),
            (4096 , 0.05),
            (8192 , 0.03),
            (16384, 0.02)
        ];
    }
}