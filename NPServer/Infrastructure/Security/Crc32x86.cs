using NPServer.Infrastructure.Helper;
using System;
using System.Runtime.Intrinsics.X86;

namespace NPServer.Infrastructure.Security
{
    /// <summary>
    /// Tiện ích bảo mật xử lý checksum cho dữ liệu.
    /// </summary>
    public static class Crc32x86
    {
        private const uint Polynomial = 0xedb88320;
        private static readonly uint[] SoftwareCrc32Table = InitializeCrc32Table();

        /// <summary>
        /// Tính checksum sử dụng thuật toán CRC32 với lệnh phần cứng SSE4.2 nếu có hỗ trợ.
        /// </summary>
        /// <param name="data">Dữ liệu cần tính checksum.</param>
        /// <returns>Giá trị checksum CRC32.</returns>
        public static uint CalculateCrc32(ReadOnlySpan<byte> data)
        {
            uint crc = 0xffffffff;

            if (Sse42.IsSupported)
            {
                foreach (byte b in data)
                {
                    crc = Sse42.Crc32(crc, b); // Sử dụng lệnh SSE4.2
                }
            }
            else
            {
                // Kiểm tra và in các giá trị bảng tra cứu và crc tính toán
                foreach (byte b in data)
                {
                    byte tableIndex = (byte)(crc & 0xff ^ b);
                    crc = crc >> 8 ^ SoftwareCrc32Table[tableIndex];
                }
            }

            return ~crc;
        }

        /// <summary>
        /// Bảng tra cứu CRC32 dành cho chế độ phần mềm.
        /// </summary>
        private static uint[] InitializeCrc32Table()
        {
            uint[] table = new uint[256];

            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (uint j = 8; j > 0; j--)
                {
                    if ((crc & 1) == 1)
                        crc = crc >> 1 ^ Polynomial;
                    else
                        crc >>= 1;
                }
                table[i] = crc;
            }

            return table;
        }

        /// <summary>
        /// Thêm checksum vào mảng byte.
        /// </summary>
        /// <param name="data">Dữ liệu cần thêm checksum.</param>
        /// <returns>Mảng byte bao gồm dữ liệu gốc và checksum.</returns>
        public static byte[] AddCrc32(string data) => AddCrc32(ConverterHelper.ToByteArray(data));

        /// <summary>
        /// Thêm checksum vào mảng byte.
        /// </summary>
        /// <param name="data">Dữ liệu cần thêm checksum.</param>
        /// <returns>Mảng byte bao gồm dữ liệu gốc và checksum.</returns>
        public static byte[] AddCrc32(byte[] data)
        {
            uint checksum = CalculateCrc32(data);
            byte[] result = new byte[data.Length + sizeof(uint) + sizeof(int)]; // Dữ liệu gốc + checksum + chiều dài dữ liệu

            // Lưu chiều dài dữ liệu (bao gồm checksum)
            BitConverter.GetBytes(result.Length).CopyTo(result, 0);

            // Sao chép dữ liệu gốc
            Buffer.BlockCopy(data, 0, result, sizeof(int), data.Length);

            // Thêm checksum vào cuối mảng
            Buffer.BlockCopy(BitConverter.GetBytes(checksum), 0, result, data.Length + sizeof(int), sizeof(uint));

            return result;
        }

        /// <summary>
        /// Kiểm tra checksum và lấy lại dữ liệu gốc nếu checksum hợp lệ.
        /// </summary>
        /// <param name="dataWithChecksum">Mảng byte bao gồm dữ liệu và checksum.</param>
        /// <param name="originalData">Dữ liệu gốc nếu checksum hợp lệ.</param>
        /// <returns>True nếu checksum hợp lệ, ngược lại False.</returns>
        public static bool VerifyCrc32(ReadOnlySpan<byte> dataWithChecksum, out byte[]? originalData)
        {
            if (dataWithChecksum.Length < sizeof(uint) + sizeof(uint)) // Kiểm tra đủ độ dài cho dữ liệu và checksum
            {
                originalData = null;
                return false;
            }

            // Lấy chiều dài của dữ liệu
            uint length = BitConverter.ToUInt32(dataWithChecksum[..sizeof(uint)]);
            if (dataWithChecksum.Length < length)
            {
                originalData = null;
                return false;
            }

            // Lấy dữ liệu gốc và checksum từ mảng
            ReadOnlySpan<byte> data = dataWithChecksum[sizeof(uint)..(sizeof(uint) + (int)length)];
            ReadOnlySpan<byte> checksumBytes = dataWithChecksum[(sizeof(uint) + (int)length)..];

            uint checksum = BitConverter.ToUInt32(checksumBytes);

            // Kiểm tra tính hợp lệ của checksum
            if (CalculateCrc32(data) == checksum)
            {
                // Trả lại dữ liệu gốc mà không có checksum và chiều dài
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
}
