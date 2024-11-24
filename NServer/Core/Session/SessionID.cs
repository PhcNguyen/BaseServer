using System;
using System.Threading;
using System.Security.Cryptography;

namespace NServer.Core.Session
{
    /// <summary>
    /// Lớp đại diện cho một ID phiên duy nhất.
    /// </summary>
    /// <remarks>
    /// Khởi tạo một phiên ID mới.
    /// </remarks>
    /// <param name="value">Giá trị của ID.</param>
    public readonly struct SessionID(uint value)
    {
        private const string Alphabet = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int Base = 36;

        private static uint _counter = 1;
        private readonly uint _value = value; // 4 byte, từ 0 đến 4,294,967,295

        /// <summary>
        /// Tạo ID mới từ một số nguyên ngẫu nhiên và số đếm tăng dần.
        /// </summary>
        /// <returns>ID gọn nhẹ.</returns>
        public static SessionID NewId()
        {
            var buffer = new byte[4];
            RandomNumberGenerator.Fill(buffer);
            uint randomValue = BitConverter.ToUInt32(buffer, 0);

            uint uniqueValue = Interlocked.Increment(ref _counter) + randomValue;
            return new SessionID(uniqueValue);
        }

        /// <summary>
        /// Chuyển đổi ID thành chuỗi Base36.
        /// </summary>
        /// <returns>Chuỗi đại diện ID.</returns>
        public override string ToString()
        {
            var value = _value;
            Span<char> buffer = stackalloc char[13]; // Tối đa cần 13 ký tự để biểu diễn uint bằng Base36
            int index = buffer.Length;

            do
            {
                buffer[--index] = Alphabet[(int)(value % Base)];
                value /= Base;
            } while (value > 0);

            // Đảm bảo độ dài của chuỗi là 7 ký tự
            return new string(buffer[index..]).PadLeft(7, '0');
        }

        /// <summary>
        /// Chuyển đổi chuỗi Base36 thành SessionID.
        /// </summary>
        /// <param name="input">Chuỗi cần chuyển đổi.</param>
        /// <returns>ID gọn nhẹ.</returns>
        public static SessionID Parse(string input)
        {
            uint value = 0;

            foreach (char c in input.ToUpperInvariant())
            {
                int charIndex = Alphabet.IndexOf(c);
                if (charIndex == -1)
                {
                    throw new ArgumentException($"Invalid character '{c}' in input string.", nameof(input));
                }
                value = (uint)(value * Base + charIndex);
            }

            return new SessionID(value);
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