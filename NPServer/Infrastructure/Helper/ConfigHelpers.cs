using System;
using System.Collections.Generic;

namespace NPServer.Infrastructure.Helper
{
    public static class ConfigHelpers
    {
        public static (int MaxRequests, TimeSpan TimeWindow) ParseRateLimit(string? rateLimitString)
        {
            if (string.IsNullOrWhiteSpace(rateLimitString))
            {
                // Giá trị mặc định khi chuỗi trống hoặc không hợp lệ
                return (10, TimeSpan.FromSeconds(0.1));
            }

            // Loại bỏ dấu ngoặc (nếu có)
            rateLimitString = rateLimitString.Trim('(', ')');

            var parts = rateLimitString.Split(',');

            // Xử lý các thành phần và kiểm tra định dạng
            if (parts.Length == 2 &&
                int.TryParse(parts[0].Trim(), out var maxRequests) &&
                TimeSpan.TryParse(parts[1].Trim(), out var timeWindow))
            {
                return (maxRequests, timeWindow);
            }
            else
            {
                throw new FormatException($"Dữ liệu không hợp lệ cho kiểu (int, TimeSpan): {rateLimitString}");
            }
        }

        public static (int BufferSize, double AllocationValue)[] ParseBufferAllocations(string? bufferAllocations)
        {
            if (string.IsNullOrWhiteSpace(bufferAllocations))
            {
                // Trả về một mảng rỗng nếu đầu vào trống
                return [];
            }

            var allocations = bufferAllocations.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var result = new List<(int BufferSize, double AllocationValue)>();

            foreach (var allocation in allocations)
            {
                var parts = allocation.Split(':', StringSplitOptions.RemoveEmptyEntries); // Tách bằng dấu ':'

                if (parts.Length == 2 &&
                    int.TryParse(parts[0].Trim(), out var bufferSize) && // Xử lý BufferSize
                    double.TryParse(parts[1].Trim(), out var allocationValue)) // Xử lý AllocationValue
                {
                    result.Add((bufferSize, allocationValue));
                }
                else
                {
                    throw new FormatException($"Dữ liệu không hợp lệ cho BufferAllocations: {allocation}");
                }
            }

            return [.. result];
        }
    }
}
