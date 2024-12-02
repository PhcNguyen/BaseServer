using System;
using System.Security.Cryptography;
using System.Text;

namespace NServer.Infrastructure.Security
{
    /// <summary>
    /// Lớp RsaCipher cung cấp các chức năng mã hóa và giải mã sử dụng thuật toán RSA.
    /// </summary>
    public class RsaCryptography : IDisposable
    {
        private readonly RSA _rsa;
        private bool _disposed;

        /// <summary>
        /// Khởi tạo lớp RsaCipher với một cặp khóa RSA.
        /// </summary>
        public RsaCryptography()
        {
            _rsa = RSA.Create();
        }

        /// <summary>
        /// Tạo và trả về một cặp khóa RSA mới.
        /// </summary>
        /// <param name="keySize">Kích thước của khóa RSA (mặc định là 2048 bit).</param>
        /// <returns>Cặp khóa RSA (Public và Private).</returns>
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
        public string Decrypt(RSAParameters privatekey, byte[] ciphertext)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            _rsa.ImportParameters(privatekey);
            return Encoding.UTF8.GetString(_rsa.Decrypt(ciphertext, RSAEncryptionPadding.Pkcs1));
        }

        /// <summary>
        /// Giải phóng tài nguyên.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
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

        ~RsaCryptography()
        {
            Dispose(disposing: false);
        }
    }
}