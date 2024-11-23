using System.Text;

namespace ToolClient.Core.Network
{
    /// <summary>
    /// Provides helper methods for converting between byte arrays and different data types.
    /// </summary>
    internal class ConverterHelper
    {
        /// <summary>
        /// Converts an integer to a byte array.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        /// <returns>A byte array representing the integer value.</returns>
        public static byte[] ToBytes(int value) => BitConverter.GetBytes(value);

        /// <summary>
        /// Converts a string to a byte array using UTF-8 encoding.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>A byte array representing the string.</returns>
        public static byte[] ToBytes(string str) => Encoding.UTF8.GetBytes(str);

        /// <summary>
        /// Converts a double to a byte array.
        /// </summary>
        /// <param name="value">The double value to convert.</param>
        /// <returns>A byte array representing the double value.</returns>
        public static byte[] ToBytes(double value) => BitConverter.GetBytes(value);

        /// <summary>
        /// Converts a byte array to an integer.
        /// </summary>
        /// <param name="byteArray">The byte array to convert.</param>
        /// <returns>The integer value represented by the byte array.</returns>
        public static int ToInt(byte[] byteArray) => BitConverter.ToInt32(byteArray, 0);

        /// <summary>
        /// Converts a byte array to a string using UTF-8 decoding.
        /// </summary>
        /// <param name="byteArray">The byte array to convert.</param>
        /// <returns>The string represented by the byte array.</returns>
        public static string ToString(byte[] byteArray) => Encoding.UTF8.GetString(byteArray);

        /// <summary>
        /// Converts a byte array to a double.
        /// </summary>
        /// <param name="byteArray">The byte array to convert.</param>
        /// <returns>The double value represented by the byte array.</returns>
        public static double ToDouble(byte[] byteArray) => BitConverter.ToDouble(byteArray, 0);

        /// <summary>
        /// Converts a hexadecimal string to a byte array.
        /// </summary>
        /// <param name="hex">The hexadecimal string to convert.</param>
        /// <returns>A byte array representing the hexadecimal string.</returns>
        public static byte[] HexStrToBytes(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];

            // Tối ưu hóa vòng lặp để không dùng Substring
            for (int i = 0; i < numberChars; i += 2)
            {
                // Chuyển 2 ký tự hex thành một byte
                bytes[i / 2] = (byte)((GetHexValue(hex[i]) << 4) + GetHexValue(hex[i + 1]));
            }
            return bytes;
        }

        private static int GetHexValue(char hexChar)
        {
            // Xử lý ký tự hex (0-9, A-F)
            return hexChar <= '9' ? hexChar - '0' : char.ToUpper(hexChar) - 'A' + 10;
        }

        /// <summary>
        /// Converts a byte array to a hexadecimal string.
        /// </summary>
        /// <param name="byteArray">The byte array to convert.</param>
        /// <returns>A string representing the byte array in hexadecimal format.</returns>
        public static string BytesToHexStr(byte[] byteArray)
        {
            // Tối ưu hóa capacity của StringBuilder theo độ dài của byteArray
            var hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                // Sử dụng Append thay vì AppendFormat để tối ưu
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }
    }
}