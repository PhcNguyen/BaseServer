using System;
using System.Collections.Generic;

namespace NServer.Infrastructure.Security
{
    /// <summary>
    /// Tiện ích bảo mật xử lý checksum cho dữ liệu.
    /// </summary>
    public static class Crc32Checksum
    {
        private static readonly uint[] _crc32Table = InitializeCrc32Table();

        public static IReadOnlyList<uint> Crc32Table => _crc32Table;

        private static uint[] InitializeCrc32Table()
        {
            uint[] table = new uint[256];
            uint polynomial = 0xedb88320;

            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (uint j = 8; j > 0; j--)
                {
                    if ((crc & 1) == 1)
                        crc = crc >> 1 ^ polynomial;
                    else
                        crc >>= 1;
                }
                table[i] = crc;
            }

            return table;
        }

        /// <summary>
        /// Tính checksum sử dụng thuật toán CRC32.
        /// </summary>
        public static uint CalculateCrc32(byte[] data)
        {
            uint crc = 0xffffffff;
            foreach (byte b in data)
            {
                byte tableIndex = (byte)(crc & 0xff ^ b);
                crc = crc >> 8 ^ Crc32Table[tableIndex];
            }
            return ~crc;
        }

        /// <summary>
        /// Thêm checksum vào mảng byte.
        /// </summary>
        public static byte[] AddCrc32(byte[] data)
        {
            uint checksum = CalculateCrc32(data);
            byte[] result = new byte[data.Length + sizeof(uint)];
            Array.Copy(data, result, data.Length);
            Array.Copy(BitConverter.GetBytes(checksum), 0, result, data.Length, sizeof(uint));
            return result;
        }

        /// <summary>
        /// Kiểm tra checksum và lấy lại dữ liệu gốc nếu checksum hợp lệ.
        /// </summary>
        public static bool VerifyCrc32(byte[] dataWithChecksum, out byte[]? originalData)
        {
            if (dataWithChecksum.Length < sizeof(uint))
            {
                originalData = null;
                return false;
            }

            byte[] data = new byte[dataWithChecksum.Length - sizeof(uint)];
            byte[] checksumBytes = new byte[sizeof(uint)];
            Array.Copy(dataWithChecksum, data, data.Length);
            Array.Copy(dataWithChecksum, data.Length, checksumBytes, 0, checksumBytes.Length);

            uint checksum = BitConverter.ToUInt32(checksumBytes, 0);

            if (CalculateCrc32(data) == checksum)
            {
                originalData = data;
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