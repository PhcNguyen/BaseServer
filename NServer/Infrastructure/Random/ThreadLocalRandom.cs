using System;
using System.Threading;

namespace NServer.Infrastructure.Random;

/// <summary>
/// Cung cấp các phương thức sinh số ngẫu nhiên trong môi trường đa luồng.
/// </summary>
public static class ThreadLocalRandom
{
    // Trạng thái cục bộ cho mỗi luồng
    private static readonly ThreadLocal<ulong> _state = new(() => (ulong)DateTime.UtcNow.Ticks);

    /// <summary>
    /// Sinh ra số ngẫu nhiên 64-bit bằng thuật toán xorshift.
    /// </summary>
    private static ulong NextUInt64()
    {
        ulong x = _state.Value;
        x ^= x << 13;
        x ^= x >> 7;
        x ^= x << 17;
        _state.Value = x;
        return x;
    }

    /// <summary>
    /// Điền các byte ngẫu nhiên vào buffer.
    /// </summary>
    /// <param name="buffer">Mảng byte để điền dữ liệu ngẫu nhiên.</param>
    public static void Fill(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        int length = buffer.Length;
        int i = 0;

        while (length >= 8)
        {
            ulong randomValue = NextUInt64();
            Buffer.BlockCopy(BitConverter.GetBytes(randomValue), 0, buffer, i, 8);
            i += 8;
            length -= 8;
        }

        if (length > 0)
        {
            ulong randomValue = NextUInt64();
            for (int j = 0; j < length; j++)
            {
                buffer[i + j] = (byte)((randomValue >> (j * 8)) & 0xFF);
            }
        }
    }
}