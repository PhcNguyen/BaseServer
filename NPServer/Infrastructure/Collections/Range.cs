using System;

namespace NPServer.Infrastructure.Collections;

/// <summary>
/// Đại diện cho một phạm vi với giới hạn dưới (Min) và giới hạn trên (Max).
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của phạm vi, phải triển khai <see cref="IComparable{T}"/>.</typeparam>
/// <remarks>
/// Khởi tạo một đối tượng <see cref="Range{T}"/> mới với giới hạn dưới và giới hạn trên được chỉ định.
/// </remarks>
/// <param name="min">Giá trị giới hạn dưới của phạm vi.</param>
/// <param name="max">Giá trị giới hạn trên của phạm vi.</param>
public struct Range<T>(T min, T max) where T : IComparable<T>
{
    /// <summary>
    /// Lấy giá trị giới hạn dưới của phạm vi.
    /// </summary>
    public T Min { get; private set; } = min;

    /// <summary>
    /// Lấy giá trị giới hạn trên của phạm vi.
    /// </summary>
    public T Max { get; private set; } = max;

    /// <summary>
    /// Kiểm tra xem phạm vi hiện tại có giao nhau với một phạm vi khác hay không.
    /// </summary>
    /// <param name="other">Phạm vi khác để kiểm tra giao nhau.</param>
    /// <returns>True nếu hai phạm vi giao nhau; ngược lại, false.</returns>
    public readonly bool Intersects(Range<T> other)
    {
        return (Min.CompareTo(other.Max) <= 0) && (Max.CompareTo(other.Min) >= 0);
    }
}
