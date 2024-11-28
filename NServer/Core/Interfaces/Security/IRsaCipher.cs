using System.Security.Cryptography;

namespace Base.Core.Interfaces.Security
{
    internal interface IRsaCipher
    {
        /// <summary>
        /// Thuộc tính khóa riêng của RSA.
        /// </summary>
        RSAParameters PrivateKey { get; }

        /// <summary>
        /// Thuộc tính khóa công khai của RSA.
        /// </summary>
        RSAParameters PublicKey { get; }

        /// <summary>
        /// Khởi tạo và tải các khóa RSA nếu cần.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Mã hóa văn bản bằng khóa công khai của máy chủ.
        /// </summary>
        /// <param name="plaintext">Chuỗi văn bản cần mã hóa.</param>
        /// <returns>Mảng byte chứa dữ liệu đã được mã hóa.</returns>
        byte[] Encrypt(string plaintext);

        /// <summary>
        /// Giải mã dữ liệu đã mã hóa bằng khóa bí mật của máy chủ.
        /// </summary>
        /// <param name="encryptedData">Mảng byte chứa dữ liệu đã mã hóa.</param>
        /// <returns>Chuỗi văn bản đã được giải mã.</returns>
        string Decrypt(byte[] encryptedData);
    }
}
