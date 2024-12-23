using System;
using System.Text;

namespace NPServer.Infrastructure.Helper;

/// <summary>
/// Cung cấp các phương thức trợ giúp để chuyển đổi giữa các mảng byte và các kiểu dữ liệu khác nhau.
/// </summary>
public static class ConverterHelper
{
    /// <summary>
    /// Chuyển đổi một số nguyên thành mảng byte.
    /// </summary>
    /// <param name="value">Giá trị số nguyên cần chuyển đổi.</param>
    /// <returns>Mảng byte đại diện cho giá trị số nguyên.</returns>
    public static byte[] ToByteArray(int value) => BitConverter.GetBytes(value);

    /// <summary>
    /// Chuyển đổi một chuỗi thành mảng byte sử dụng mã hóa UTF-8.
    /// </summary>
    /// <param name="str">Chuỗi cần chuyển đổi.</param>
    /// <returns>Mảng byte đại diện cho chuỗi.</returns>
    public static byte[] ToByteArray(string str) => Encoding.UTF8.GetBytes(str);

    /// <summary>
    /// Chuyển đổi một số thực thành mảng byte.
    /// </summary>
    /// <param name="value">Giá trị số thực cần chuyển đổi.</param>
    /// <returns>Mảng byte đại diện cho giá trị số thực.</returns>
    public static byte[] ToByteArray(double value) => BitConverter.GetBytes(value);

    /// <summary>
    /// Chuyển đổi một mảng byte thành số nguyên.
    /// </summary>
    /// <param name="byteArray">Mảng byte cần chuyển đổi.</param>
    /// <returns>Giá trị số nguyên được đại diện bởi mảng byte.</returns>
    public static int ToInt(byte[] byteArray) => BitConverter.ToInt32(byteArray, 0);

    /// <summary>
    /// Chuyển đổi một mảng byte thành chuỗi sử dụng mã hóa UTF-8.
    /// </summary>
    /// <param name="byteArray">Mảng byte cần chuyển đổi.</param>
    /// <returns>Chuỗi được đại diện bởi mảng byte.</returns>
    public static string ToString(byte[] byteArray) => Encoding.UTF8.GetString(byteArray);

    /// <summary>
    /// Chuyển đổi một mảng byte thành số thực.
    /// </summary>
    /// <param name="byteArray">Mảng byte cần chuyển đổi.</param>
    /// <returns>Giá trị số thực được đại diện bởi mảng byte.</returns>
    public static double ToDouble(byte[] byteArray) => BitConverter.ToDouble(byteArray, 0);

    /// <summary>
    /// Chuyển đổi một chuỗi hex thành mảng byte.
    /// </summary>
    /// <param name="hex">Chuỗi hex cần chuyển đổi.</param>
    /// <returns>Mảng byte đại diện cho chuỗi hex.</returns>
    public static byte[] HexStrToBytes(string hex)
    {
        int numberChars = hex.Length;
        byte[] bytes = new byte[numberChars / 2];

        for (int i = 0; i < numberChars; i += 2)
        {
            bytes[i / 2] = (byte)((GetHexValue(hex[i]) << 4) + GetHexValue(hex[i + 1]));
        }
        return bytes;
    }

    private static int GetHexValue(char hexChar)
    {
        // Xử lý ký tự hex (0-9, A-F)
        return hexChar <= '9' ? hexChar - '0' : (char.ToUpper(hexChar) - 'A' + 10);
    }

    /// <summary>
    /// Chuyển đổi một mảng byte thành chuỗi hex.
    /// </summary>
    /// <param name="byteArray">Mảng byte cần chuyển đổi.</param>
    /// <returns>Chuỗi đại diện cho mảng byte trong định dạng hex.</returns>
    public static string BytesToHexStr(byte[] byteArray)
    {
        var hex = new StringBuilder(byteArray.Length * 2);
        foreach (byte b in byteArray)
        {
            hex.AppendFormat("{0:x2}", b);
        }
        return hex.ToString();
    }
}