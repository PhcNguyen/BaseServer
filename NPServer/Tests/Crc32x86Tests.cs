using NPServer.Infrastructure.Security;
using System;

namespace NPServer.Tests
{
    public class Crc32x86Tests
    {
        public static void Main()
        {
            Test_CalculateCrc32();
            Test_AddCrc32();
            Test_VerifyCrc32_Valid();
            Test_VerifyCrc32_Invalid();
        }

        public static void Test_CalculateCrc32()
        {
            // Arrange
            byte[] data = { 1, 2, 3, 4, 5 };

            // Act
            uint checksum = Crc32x86.CalculateCrc32(data);

            // Assert
            if (checksum == 0xCBF43926)
                Console.WriteLine("Test_CalculateCrc32 Passed");
            else
                Console.WriteLine("Test_CalculateCrc32 Failed");
        }

        public static void Test_AddCrc32()
        {
            // Arrange
            byte[] data = { 1, 2, 3, 4, 5 };

            // Act
            byte[] dataWithChecksum = Crc32x86.AddCrc32(data);

            // Assert
            if (dataWithChecksum.Length == data.Length + sizeof(uint))
                Console.WriteLine("Test_AddCrc32 Passed");
            else
                Console.WriteLine("Test_AddCrc32 Failed");
        }

        public static void Test_VerifyCrc32_Valid()
        {
            // Arrange
            byte[] data = { 1, 2, 3, 4, 5 };
            byte[] dataWithChecksum = Crc32x86.AddCrc32(data);

            // Act
            bool isValid = Crc32x86.VerifyCrc32(dataWithChecksum, out byte[]? originalData);

            // Assert
            if (isValid && originalData != null && originalData.Length == data.Length)
                Console.WriteLine("Test_VerifyCrc32_Valid Passed");
            else
                Console.WriteLine("Test_VerifyCrc32_Valid Failed");
        }

        public static void Test_VerifyCrc32_Invalid()
        {
            // Arrange
            byte[] data = { 1, 2, 3, 4, 5 };
            byte[] dataWithChecksum = Crc32x86.AddCrc32(data);
            // Thay đổi dữ liệu để kiểm tra tính hợp lệ của checksum
            dataWithChecksum[0] = 9;

            // Act
            bool isValid = Crc32x86.VerifyCrc32(dataWithChecksum, out byte[]? originalData);

            // Assert
            if (!isValid)
                Console.WriteLine("Test_VerifyCrc32_Invalid Passed");
            else
                Console.WriteLine("Test_VerifyCrc32_Invalid Failed");
        }
    }
}
