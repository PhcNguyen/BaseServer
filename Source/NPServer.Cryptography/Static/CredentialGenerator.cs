using NPServer.Cryptography.Algorithm;
using System;
using System.Security.Cryptography;

namespace NPServer.Cryptography.Static;

/// <summary>
/// Lớp cung cấp các phương thức hỗ trợ xác thực.
/// </summary>
public static class CredentialGenerator
{
    /// <summary>
    /// Trả về một chuỗi muối ngẫu nhiên và bộ xác thực mật khẩu SRP6 cho email và mật khẩu văn bản rõ được cung cấp.
    /// </summary>
    /// <param name="email">Email của người dùng.</param>
    /// <param name="password">Mật khẩu của người dùng.</param>
    /// <returns>Chuỗi muối và bộ xác thực dưới dạng chuỗi hex.</returns>
    public static (string salt, string verifier) GenerateSaltAndVerifier(string email, string password)
    {
        byte[] s = RandomNumberGenerator.GetBytes((int)16u);
        byte[] v = Srp6.GenerateVerifier(s, email.ToLower(), password);
        return (Convert.ToHexString(s), Convert.ToHexString(v));
    }
}
