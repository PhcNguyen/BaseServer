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
        public static readonly Dictionary<int, double> BufferAllocations = new()
        {
            { 256, 0.40 },   // Increased allocation for smaller buffers
            { 512, 0.25 },   // Adjusted to reflect usage
            { 1024, 0.15 },
            { 2048, 0.10 },
            { 4096, 0.05 },  // Reduced allocation for medium buffers
            { 8192, 0.03 },  // Smallest allocation for larger buffers
            { 16384, 0.02 }  // Smallest allocation for the largest buffer
        };
    }
}