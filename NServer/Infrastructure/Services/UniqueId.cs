using System;
using System.Security.Cryptography;
using System.Threading;

namespace NServer.Infrastructure.Services
{
    /// <summary>
    /// Lớp đại diện cho một ID phiên duy nhất.
    /// </summary>
    /// <remarks>
    /// Khởi tạo một phiên ID mới.
    /// </remarks>
    /// <param name="value">Giá trị của ID.</param>
    public readonly struct UniqueId(uint value)
    {
        private const string Alphabet = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int Base = 36;

        private static uint _counter = 1;
        private readonly uint _value = value; // 4 byte, từ 0 đến 4,294,967,295

        /// <summary>
        /// Tạo ID mới từ một số nguyên ngẫu nhiên và số đếm tăng dần.
        /// </summary>
        /// <returns>ID gọn nhẹ.</returns>
        public static UniqueId NewId()
        {
            var buffer = new byte[4];
            RandomNumberGenerator.Fill(buffer);
            uint randomValue = BitConverter.ToUInt32(buffer, 0);

            uint uniqueValue = Interlocked.Increment(ref _counter) + randomValue;
            return new UniqueId(uniqueValue);
        }

        /// <summary>
        /// Chuyển đổi ID thành chuỗi Base36.
        /// </summary>
        /// <returns>Chuỗi đại diện ID.</returns>
        public override string ToString()
        {
            var mvalue = _value;
            const int bufferSize = 13; // Tối đa cần 13 ký tự để biểu diễn uint bằng Base36
            Span<char> buffer = stackalloc char[bufferSize];
            int index = buffer.Length;

            // Tính toán ký tự Base36
            do
            {
                buffer[--index] = Alphabet[(int)(mvalue % Base)];
                mvalue /= Base;
            } while (mvalue > 0);

            // Đảm bảo độ dài của chuỗi là 7 ký tự
            return new string(buffer[index..]).PadLeft(7, '0');
        }

        /// <summary>
        /// Chuyển đổi chuỗi Base36 thành SessionID.
        /// </summary>
        /// <param name="input">Chuỗi cần chuyển đổi.</param>
        /// <returns>ID gọn nhẹ.</returns>
        public static UniqueId Parse(string input)
        {
            uint value = 0;

            foreach (char c in input.ToUpperInvariant())
            {
                int charIndex = Alphabet.IndexOf(c);
                if (charIndex == -1)
                {
                    throw new ArgumentException($"Invalid character '{c}' in input string.", nameof(input));
                }
                value = (uint)((value * Base) + charIndex);
            }

            return new UniqueId(value);
        }

        /// <summary>
        /// Trả về giá trị số nguyên gốc.
        /// </summary>
        public uint ToValue()
        {
            return _value;
        }
    }
}