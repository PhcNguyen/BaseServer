using System;

namespace NServer.Core.Security
{
    /// <summary>
    /// Tiện ích bảo mật xử lý checksum cho dữ liệu.
    /// </summary>
    internal static class PacketSecurity
    {
        /// <summary>
        /// Thêm checksum vào mảng byte.
        /// </summary>
        public static byte[] AddChecksum(byte[] data)
        {
            uint checksum = CalculateChecksum(data);
            byte[] result = new byte[data.Length + sizeof(uint)];
            Array.Copy(data, result, data.Length);
            Array.Copy(BitConverter.GetBytes(checksum), 0, result, data.Length, sizeof(uint));
            return result;
        }

        /// <summary>
        /// Tính checksum sử dụng thuật toán CRC32.
        /// </summary>
        public static uint CalculateChecksum(byte[] data)
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
        /// Kiểm tra checksum và lấy lại dữ liệu gốc nếu checksum hợp lệ.
        /// </summary>
        public static bool VerifyChecksum(byte[] dataWithChecksum, out byte[]? originalData)
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

            if (CalculateChecksum(data) == checksum)
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

        private static readonly uint[] Crc32Table;

        static PacketSecurity()
        {
            Crc32Table = new uint[256];
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
                Crc32Table[i] = crc;
            }
        }
    }
}
