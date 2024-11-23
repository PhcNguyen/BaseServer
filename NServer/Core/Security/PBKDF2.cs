using System;
using System.Security.Cryptography;

namespace NServer.Core.Security
{
    internal class PBKDF2
    {
        private const int SaltSize = 16;       // Kích thước salt (16 bytes)
        private const int KeySize = 32;        // Kích thước key (32 bytes)
        private const int Iterations = 100000; // Số vòng lặp

        // Hàm tạo hash từ mật khẩu
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            }

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(KeySize);

            return Convert.ToBase64String(salt) + "|" + Convert.ToBase64String(hash);
        }

        // Hàm xác thực mật khẩu
        public static bool VerifyPassword(string hashedPassword, string inputPassword)
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
}