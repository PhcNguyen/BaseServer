using System.IO;
using System.Security.Cryptography;

namespace NPServer.Infrastructure.Helper;

/// <summary>
/// Cung cấp các phương thức trợ giúp cho mã hóa và giải mã dữ liệu, cũng như quản lý mật khẩu và khóa mã hóa.
/// </summary>
public static class CryptographyHelper
{
    private const int PasswordKeySize = 64;
    private const int PasswordIterationCount = 210000;  // Số lần lặp lại hợp lý cho PBKDF2-HMAC-SHA512 theo các khuyến nghị của OWASP 2023

    /// <summary>
    /// Băm mật khẩu và tạo ra một giá trị salt.
    /// </summary>
    /// <param name="password">Mật khẩu cần băm.</param>
    /// <param name="salt">Giá trị salt ngẫu nhiên được tạo.</param>
    /// <returns>Mảng byte đại diện cho giá trị băm của mật khẩu.</returns>
    public static byte[] HashPassword(string password, out byte[] salt)
    {
        salt = RandomNumberGenerator.GetBytes(PasswordKeySize);
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, PasswordIterationCount, HashAlgorithmName.SHA512, PasswordKeySize);
    }

    /// <summary>
    /// Xác minh mật khẩu bằng cách so sánh giá trị băm với giá trị băm được tạo từ mật khẩu và salt.
    /// </summary>
    /// <param name="password">Mật khẩu cần xác minh.</param>
    /// <param name="hash">Giá trị băm mật khẩu.</param>
    /// <param name="salt">Giá trị salt tương ứng.</param>
    /// <returns>True nếu mật khẩu được xác minh thành công, ngược lại là false.</returns>
    public static bool VerifyPassword(string password, byte[] hash, byte[] salt)
    {
        byte[] hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, salt, PasswordIterationCount, HashAlgorithmName.SHA512, PasswordKeySize);
        return CryptographicOperations.FixedTimeEquals(hashToCompare, hash);
    }

    /// <summary>
    /// Tạo một token ngẫu nhiên có kích thước nhất định.
    /// </summary>
    /// <param name="size">Kích thước của token (mặc định là 32 byte).</param>
    /// <returns>Mảng byte đại diện cho token ngẫu nhiên.</returns>
    public static byte[] GenerateToken(int size = 32)
    {
        return RandomNumberGenerator.GetBytes(size);
    }

    /// <summary>
    /// Tạo một khóa AES có kích thước nhất định.
    /// </summary>
    /// <param name="size">Kích thước của khóa AES (mặc định là 256 bit).</param>
    /// <returns>Mảng byte đại diện cho khóa AES.</returns>
    public static byte[] GenerateAesKey(int size = 256)
    {
        using Aes aesAlgorithm = Aes.Create();
        aesAlgorithm.KeySize = size;
        aesAlgorithm.GenerateKey();
        return aesAlgorithm.Key;
    }

    /// <summary>
    /// Mã hóa token bằng AES với khóa và tạo IV ngẫu nhiên.
    /// </summary>
    /// <param name="tokenToEncrypt">Token cần mã hóa.</param>
    /// <param name="key">Khóa mã hóa AES.</param>
    /// <param name="iv">Vector khởi tạo (IV) ngẫu nhiên.</param>
    /// <returns>Mảng byte đại diện cho token đã mã hóa.</returns>
    public static byte[] EncryptToken(byte[] tokenToEncrypt, byte[] key, out byte[] iv)
    {
        using Aes aesAlgorithm = Aes.Create();
        aesAlgorithm.Key = key;
        aesAlgorithm.GenerateIV();
        iv = aesAlgorithm.IV;
        ICryptoTransform encryptor = aesAlgorithm.CreateEncryptor();

        using MemoryStream memoryStream = new();
        using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
        cryptoStream.Write(tokenToEncrypt, 0, tokenToEncrypt.Length);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Giải mã token đã được mã hóa bằng AES.
    /// </summary>
    /// <param name="encryptedToken">Token đã được mã hóa.</param>
    /// <param name="key">Khóa mã hóa AES.</param>
    /// <param name="iv">Vector khởi tạo (IV) tương ứng.</param>
    /// <param name="decryptedToken">Token giải mã thành công hoặc null nếu thất bại.</param>
    /// <returns>True nếu giải mã thành công, ngược lại là false.</returns>
    public static bool TryDecryptToken(byte[] encryptedToken, byte[] key, byte[] iv, out byte[]? decryptedToken)
    {
        using Aes aesAlgorithm = Aes.Create();
        aesAlgorithm.Key = key;
        aesAlgorithm.IV = iv;
        ICryptoTransform decryptor = aesAlgorithm.CreateDecryptor();

        try
        {
            using MemoryStream memoryStream = new(encryptedToken);
            using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
            using MemoryStream decryptionBuffer = new();
            cryptoStream.CopyTo(decryptionBuffer);
            decryptedToken = decryptionBuffer.ToArray();
            return true;
        }
        catch
        {
            decryptedToken = null;
            return false;
        }
    }

    /// <summary>
    /// Xác minh sự khớp của hai token bằng cách sử dụng so sánh thời gian cố định.
    /// </summary>
    /// <param name="credentialsToken">Token từ tài khoản người dùng.</param>
    /// <param name="sessionToken">Token phiên làm việc.</param>
    /// <returns>True nếu hai token khớp, ngược lại là false.</returns>
    public static bool VerifyToken(byte[] credentialsToken, byte[] sessionToken)
    {
        return CryptographicOperations.FixedTimeEquals(credentialsToken, sessionToken);
    }
}