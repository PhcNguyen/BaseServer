using System.Threading.Tasks;

namespace NServer.Core.Interfaces.Security
{
    /// <summary>
    /// A class that provides AES encryption and decryption functionality using a key.
    /// Implements IDisposable to release resources.
    /// </summary>
    internal interface IAesCipher
    {
        /// <summary>
        /// Gets the encryption key used for AES encryption and decryption.
        /// </summary>
        byte[] Key { get; }

        /// <summary>
        /// Encrypts the provided plaintext using AES encryption in CTR mode.
        /// </summary>
        /// <param name="plaintext">The data to be encrypted.</param>
        /// <returns>The encrypted ciphertext.</returns>
        byte[] Encrypt(byte[] plaintext);

        /// <summary>
        /// Decrypts the provided ciphertext using AES decryption in CTR mode.
        /// </summary>
        /// <param name="cipherText">The data to be decrypted.</param>
        /// <returns>The decrypted plaintext.</returns>
        byte[] Decrypt(byte[] ciphertext);

        /// <summary>
        /// Encrypts the provided plaintext asynchronously using AES encryption in CTR mode.
        /// </summary>
        /// <param name="plaintext">The data to be encrypted.</param>
        /// <returns>A task that represents the asynchronous encryption operation, with the encrypted data as the result.</returns>
        ValueTask<byte[]> EncryptAsync(byte[] plaintext);

        /// <summary>
        /// Decrypts the provided ciphertext asynchronously using AES decryption in CTR mode.
        /// </summary>
        /// <param name="cipherText">The data to be decrypted.</param>
        /// <returns>A task that represents the asynchronous decryption operation, with the decrypted data as the result.</returns>
        ValueTask<byte[]> DecryptAsync(byte[] ciphertext);

        /// <summary>
        /// Disposes of the <see cref="AesCipher"/> instance.
        /// </summary>
        void Dispose();
    }
}
