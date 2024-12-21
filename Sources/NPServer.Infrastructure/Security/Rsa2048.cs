using System;
using System.Security.Cryptography;
using System.Text;

namespace NPServer.Infrastructure.Security;

/// <summary>
/// Lớp <see cref="Rsa2048"/> cung cấp các chức năng mã hóa và giải mã sử dụng thuật toán RSA.
/// </summary>
public sealed class Rsa2048 : IDisposable
{
    private readonly RSA _rsa;
    private bool _disposed;

    /// <summary>
    /// Khởi tạo một đối tượng <see cref="Rsa2048"/> với một cặp khóa RSA.
    /// </summary>
    public Rsa2048()
    {
        _rsa = RSA.Create();
    }

    /// <summary>
    /// Tạo và trả về một cặp khóa RSA mới.
    /// </summary>
    /// <param name="keySize">Kích thước của khóa RSA (mặc định là 2048 bit).</param>
    /// <returns>
    /// Một bộ giá trị chứa:
    /// <list type="bullet">
    /// <item>
    /// <term>Khóa công khai</term>
    /// <description>Được sử dụng để mã hóa dữ liệu.</description>
    /// </item>
    /// <item>
    /// <term>Khóa bí mật</term>
    /// <description>Được sử dụng để giải mã dữ liệu.</description>
    /// </item>
    /// </list>
    /// </returns>
    public static (RSAParameters PublicKey, RSAParameters PrivateKey) GenerateKeys(int keySize = 2048)
    {
        using var rsa = RSA.Create();
        rsa.KeySize = keySize;
        return (rsa.ExportParameters(false), rsa.ExportParameters(true));
    }

    /// <summary>
    /// Mã hóa văn bản bằng khóa công khai.
    /// </summary>
    /// <param name="publickey">Khóa công khai RSA.</param>
    /// <param name="plaintext">Chuỗi văn bản cần mã hóa.</param>
    /// <returns>Mảng byte chứa dữ liệu đã được mã hóa.</returns>
    /// <exception cref="ObjectDisposedException">
    /// Được ném ra nếu đối tượng <see cref="Rsa2048"/> đã bị giải phóng.
    /// </exception>
    public byte[] Encrypt(RSAParameters publickey, string plaintext)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        using var rsaEncryptor = RSA.Create();
        rsaEncryptor.ImportParameters(publickey);

        // Sử dụng OAEP padding thay cho PKCS1
        return rsaEncryptor.Encrypt(Encoding.UTF8.GetBytes(plaintext), RSAEncryptionPadding.OaepSHA256);
    }

    /// <summary>
    /// Giải mã dữ liệu đã mã hóa bằng khóa bí mật.
    /// </summary>
    /// <param name="privatekey">Khóa bí mật RSA.</param>
    /// <param name="ciphertext">Mảng byte chứa dữ liệu đã mã hóa.</param>
    /// <returns>Chuỗi văn bản đã được giải mã.</returns>
    /// <exception cref="ObjectDisposedException">
    /// Được ném ra nếu đối tượng <see cref="Rsa2048"/> đã bị giải phóng.
    /// </exception>
    public string Decrypt(RSAParameters privatekey, byte[] ciphertext)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _rsa.ImportParameters(privatekey);
        return Encoding.UTF8.GetString(_rsa.Decrypt(ciphertext, RSAEncryptionPadding.OaepSHA256));
    }

    /// <summary>
    /// Giải phóng tài nguyên được quản lý và không được quản lý.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Giải phóng tài nguyên.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> nếu giải phóng tài nguyên được quản lý; 
    /// <c>false</c> nếu chỉ giải phóng tài nguyên không được quản lý.
    /// </param>
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Giải phóng tài nguyên có thể quản lý (managed resources) tại đây
            }

            // Giải phóng tài nguyên không thể quản lý (unmanaged resources) tại đây, nếu có

            _disposed = true;
        }
    }

    /// <summary>
    /// Phương thức hủy của lớp <see cref="Rsa2048"/>.
    /// </summary>
    /// <remarks>
    /// Được gọi khi đối tượng <see cref="Rsa2048"/> bị thu hồi bởi Garbage Collector.
    /// Giải phóng tài nguyên không được quản lý.
    /// </remarks>
    ~Rsa2048()
    {
        Dispose(disposing: false);
    }
}