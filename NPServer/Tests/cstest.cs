using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NPServer.Tests;

public class ChecksumUtility
{
    private static readonly uint[] Crc32Table;

    static ChecksumUtility()
    {
        Crc32Table = new uint[256];
        const uint Polynomial = 0xedb88320;
        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (uint j = 8; j > 0; j--)
            {
                if ((crc & 1) == 1)
                {
                    crc = (crc >> 1) ^ Polynomial;
                }
                else
                {
                    crc >>= 1;
                }
            }
            Crc32Table[i] = crc;
        }
    }

    private static uint ComputeCrc32(byte[] data)
    {
        uint crc = 0xffffffff;
        foreach (byte t in data)
        {
            byte tableIndex = (byte)((crc ^ t) & 0xff);
            crc = (crc >> 8) ^ Crc32Table[tableIndex];
        }
        return ~crc;
    }

    public static byte[] AddLengthAndChecksum(byte[] data)
    {
        byte[] lengthBytes = BitConverter.GetBytes(data.Length);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(lengthBytes);
        }

        uint crc32 = ComputeCrc32(data);
        byte[] checksumBytes = BitConverter.GetBytes(crc32);

        byte[] result = new byte[4 + data.Length + 4];
        Array.Copy(lengthBytes, 0, result, 0, 4);
        Array.Copy(data, 0, result, 4, data.Length);
        Array.Copy(checksumBytes, 0, result, 4 + data.Length, 4);

        return result;
    }

    public static byte[] VerifyAndExtractData(byte[] receivedData)
    {
        byte[] lengthBytes = new byte[4];
        Array.Copy(receivedData, 0, lengthBytes, 0, 4);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(lengthBytes);
        }
        uint originalLength = BitConverter.ToUInt32(lengthBytes, 0);

        byte[] originalData = new byte[originalLength];
        byte[] receivedChecksum = new byte[4];
        Array.Copy(receivedData, 4, originalData, 0, originalLength);
        Array.Copy(receivedData, 4 + originalLength, receivedChecksum, 0, 4);

        uint receivedCrc32 = BitConverter.ToUInt32(receivedChecksum, 0);
        uint computedCrc32 = ComputeCrc32(originalData);

        if (receivedCrc32 != computedCrc32)
        {
            throw new InvalidOperationException("Checksum không hợp lệ. Dữ liệu đã bị thay đổi.");
        }

        return originalData;
    }
}