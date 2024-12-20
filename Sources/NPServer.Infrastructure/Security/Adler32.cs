using System;

namespace NPServer.Infrastructure.Security;

/// <summary>
/// Tiện ích bảo mật xử lý checksum Adler-32 cho dữ liệu.
/// </summary>
public static class Adler32
{
    private const uint ModAdler = 65521;

    /// <summary>
    /// Tính toán checksum Adler-32.
    /// </summary>
    /// <param name="data">Dữ liệu cần tính toán checksum.</param>
    /// <returns>Checksum Adler-32.</returns>
    public static uint Compute(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            return 1;

        uint a = 1; // Phần đầu tiên của checksum.
        uint b = 0; // Phần thứ hai của checksum.

        foreach (byte value in data)
        {
            a = (a + value) % ModAdler;
            b = (b + a) % ModAdler;
        }

        // Gộp hai phần thành checksum cuối cùng.
        return b << 16 | a;
    }

    /// <summary>
    /// Thêm checksum Adler-32 vào dữ liệu.
    /// </summary>
    /// <param name="data">Dữ liệu gốc.</param>
    /// <returns>Dữ liệu kèm checksum.</returns>
    public static byte[] AddChecksum(byte[] data)
    {
        uint checksum = Compute(data);
        byte[] result = new byte[data.Length + 4];
        Buffer.BlockCopy(data, 0, result, 0, data.Length);

        // Gắn checksum vào cuối dữ liệu.
        result[data.Length] = (byte)(checksum >> 24);
        result[data.Length + 1] = (byte)(checksum >> 16);
        result[data.Length + 2] = (byte)(checksum >> 8);
        result[data.Length + 3] = (byte)checksum;

        return result;
    }

    /// <summary>
    /// Kiểm tra checksum Adler-32 và trả về dữ liệu gốc nếu hợp lệ.
    /// </summary>
    /// <param name="dataWithChecksum">Dữ liệu kèm checksum.</param>
    /// <param name="originalData">Dữ liệu gốc nếu checksum hợp lệ.</param>
    /// <returns>True nếu checksum hợp lệ, ngược lại False.</returns>
    public static bool VerifyChecksum(ReadOnlySpan<byte> dataWithChecksum, out byte[]? originalData)
    {
        if (dataWithChecksum.Length < 4)
        {
            originalData = null;
            return false;
        }

        // Tách dữ liệu và checksum.
        ReadOnlySpan<byte> data = dataWithChecksum[..^4];
        uint checksum = (uint)(
            dataWithChecksum[^4] << 24 |
            dataWithChecksum[^3] << 16 |
            dataWithChecksum[^2] << 8 |
            dataWithChecksum[^1]
        );

        // So sánh checksum đã tính với checksum có sẵn.
        if (Compute(data) == checksum)
        {
            originalData = data.ToArray();
            return true;
        }
        else
        {
            originalData = null;
            return false;
        }
    }
}