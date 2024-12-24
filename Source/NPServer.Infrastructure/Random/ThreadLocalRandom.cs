using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NPServer.Infrastructure.Random;

/// <summary>
/// Cung cấp các phương thức sinh số ngẫu nhiên hiệu suất cao và an toàn trong môi trường đa luồng.
/// </summary>
public static class ThreadLocalRandom
{
    private static readonly ThreadLocal<XorShift128Plus> _threadLocalRng =
        new(() => new XorShift128Plus());

    /// <summary>
    /// Lớp triển khai thuật toán XorShift 128+ nhanh và có chất lượng ngẫu nhiên tốt
    /// </summary>
    private sealed class XorShift128Plus
    {
        private ulong _state0;
        private ulong _state1;

        public XorShift128Plus()
        {
            // Khởi tạo trạng thái ban đầu từ nhiều nguồn entropy
            _state0 = (ulong)DateTime.UtcNow.Ticks;
            _state1 = (ulong)Environment.TickCount64;

            // Thêm entropy từ các nguồn khác nhau
            _state0 ^= (ulong)Environment.CurrentManagedThreadId;
            _state1 ^= (ulong)Guid.NewGuid().GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Generate()
        {
            ulong x = _state0;
            ulong y = _state1;

            _state0 = y;
            x ^= x << 23;
            x ^= x >> 17;
            x ^= y;
            x ^= y >> 26;
            _state1 = x;

            return x + y;
        }
    }

    /// <summary>
    /// Sinh số ngẫu nhiên 64-bit với chất lượng cao
    /// </summary>
    public static ulong NextUInt64() =>
        _threadLocalRng.Value!.Generate();

    /// <summary>
    /// Điền các byte ngẫu nhiên vào buffer với hiệu suất cao
    /// </summary>
    public static void Fill(Span<byte> buffer)
    {
        ref byte start = ref System.Runtime.InteropServices.MemoryMarshal.GetReference(buffer);
        int length = buffer.Length;
        int i = 0;

        while (length >= sizeof(ulong))
        {
            ulong randomValue = NextUInt64();
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref start, i), randomValue);
            i += sizeof(ulong);
            length -= sizeof(ulong);
        }

        if (length > 0)
        {
            ulong randomValue = NextUInt64();
            for (int j = 0; j < length; j++)
            {
                Unsafe.Add(ref start, i + j) = (byte)(randomValue >> j * 8 & 0xFF);
            }
        }
    }

    /// <summary>
    /// Sinh số nguyên ngẫu nhiên trong phạm vi chỉ định
    /// </summary>
    public static int Next(int minValue, int maxValue)
    {
        if (minValue > maxValue)
            throw new ArgumentOutOfRangeException(nameof(minValue), "The minimum value must be less than or equal to the maximum value.");

        ulong range = (ulong)(maxValue - minValue);
        ulong randomValue = NextUInt64();

        // Scale the random value within the range
        return minValue + (int)(randomValue % (range + 1));  // Ensures value is within the correct range
    }

    /// <summary>
    /// Sinh số nguyên ngẫu nhiên không âm
    /// </summary>
    public static int Next()
    {
        return Next(0, int.MaxValue);
    }
}