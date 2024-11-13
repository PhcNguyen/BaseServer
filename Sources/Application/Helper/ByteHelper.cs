using System.Text;

namespace NETServer.Application.Helper
{
    internal class ByteHelper
    {
        public static byte[] ToBytes(int value) => BitConverter.GetBytes(value);
        public static byte[] ToBytes(string str) => Encoding.UTF8.GetBytes(str);
        public static byte[] ToBytes(double value) => BitConverter.GetBytes(value);

        public static int ToInt(byte[] byteArray) => BitConverter.ToInt32(byteArray, 0);
        public static string ToString(byte[] byteArray) => Encoding.UTF8.GetString(byteArray);
        public static double ToDouble(byte[] byteArray) => BitConverter.ToDouble(byteArray, 0);

        public static byte[] HexStrToBytes(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        public static string BytesToHexStr(byte[] byteArray)
        {
            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }
    }
}
