using System;
using System.Security.Cryptography;

namespace NPServer.Infrastructure.Security;

/// <summary>
/// Cung cấp các phương thức để mã hóa và xác thực mật khẩu bằng PBKDF2 với thuật toán SHA256.
/// </summary>
public static class Pbkdf2
{
    private const int SaltSize = 16;       // Kích thước của salt (16 bytes)
    private const int KeySize = 32;        // Kích thước của key (32 bytes)
    private const int Iterations = 100000; // Số vòng lặp

    /// <summary>
    /// Tạo hash cho mật khẩu bằng PBKDF2 với salt ngẫu nhiên và thuật toán SHA256.
    /// </summary>
    /// <param name="password">Mật khẩu cần mã hóa.</param>
    /// <returns>Chuỗi chứa salt và mật khẩu đã mã hóa, ngăn cách bằng dấu '|'.</returns>
    /// <exception cref="ArgumentException">Ném ra nếu mật khẩu là null hoặc rỗng.</exception>
    public static string GenerateHash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Mật khẩu không thể null hoặc rỗng.", nameof(password));
        }

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(KeySize);

        return Convert.ToBase64String(salt) + "|" + Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Xác thực mật khẩu bằng cách so sánh mật khẩu đã mã hóa với mật khẩu đầu vào.
    /// </summary>
    /// <param name="hashedPassword">Mật khẩu đã mã hóa (salt và hash).</param>
    /// <param name="inputPassword">Mật khẩu đầu vào cần xác thực.</param>
    /// <returns>True nếu mật khẩu đúng, false nếu sai.</returns>
    public static bool ValidatePassword(string hashedPassword, string inputPassword)
    {
        var parts = hashedPassword.Split('|');
        if (parts.Length != 2)
        {
            return false;
        }

        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] hash = Convert.FromBase64String(parts[1]);

        using var pbkdf2 = new Rfc2898DeriveBytes(inputPassword, salt, Iterations, HashAlgorithmName.SHA256);
        byte[] hashToCompare = pbkdf2.GetBytes(KeySize);

        return CryptographicOperations.FixedTimeEquals(hash, hashToCompare);
    }
}