using System;
using System.IO;
using System.Buffers;
using System.Threading.Tasks;
using System.Security.Cryptography;

using Base.Core.Interfaces.Security;

namespace Base.Core.Security
{
    /// <summary>
    /// A class that provides AES encryption and decryption functionality using a key.
    /// Implements IDisposable to release resources.
    /// </summary>
    internal class AesCipher : IAesCipher, IDisposable
    {
        /// <summary>
        /// Gets the encryption key used for AES encryption and decryption.
        /// </summary>
        public byte[] Key { get; private set; }

        private bool isDisposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AesCipher"/> class with a specified key.
        /// </summary>
        /// <param name="key">The key used for AES encryption. Must be 128, 192, or 256 bits.</param>
        /// <exception cref="ArgumentException">Thrown when the key size is invalid.</exception>
        public AesCipher(byte[] key)
        {
            int keySize = key.Length * 8;
            if (keySize != 128 && keySize != 192 && keySize != 256)
            {
                throw new ArgumentException("The provided key length must be 128, 192, or 256 bits.");
            }
            Key = key;
        }

        /// <summary>
        /// Generates a random AES key with the specified size.
        /// </summary>
        /// <param name="keySize">The size of the key in bits. Must be 128, 192, or 256 bits.</param>
        /// <returns>A randomly generated AES key.</returns>
        /// <exception cref="ArgumentException">Thrown when the key size is invalid.</exception>
        public static byte[] GenerateKey(int keySize = 256)
        {
            if (keySize != 128 && keySize != 192 && keySize != 256)
            {
                throw new ArgumentException("Key size must be 128, 192, or 256 bits.");
            }

            using var rng = RandomNumberGenerator.Create();
            byte[] key = new byte[keySize / 8];
            rng.GetBytes(key);
            return key;
        }

        /// <summary>
        /// Increments the counter used for AES encryption in CTR mode.
        /// </summary>
        /// <param name="counter">The counter byte array to increment.</param>
        private static void IncrementCounter(byte[] counter)
        {
            for (int i = counter.Length - 1; i >= 0; i--)
            {
                if (++counter[i] != 0) break;
            }
        }

        /// <summary>
        /// Creates a new AES encryptor with the provided key.
        /// </summary>
        /// <param name="key">The key used for AES encryption.</param>
        /// <returns>A new <see cref="Aes"/> encryptor instance.</returns>
        private static Aes CreateAesEncryptor(byte[] key)
        {
            var aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.ECB;
            return aes;
        }

        /// <summary>
        /// Encrypts the provided plaintext using AES encryption in CTR mode.
        /// </summary>
        /// <param name="plaintext">The data to be encrypted.</param>
        /// <returns>The encrypted ciphertext.</returns>
        public byte[] Encrypt(byte[] plaintext)
        {
            using var aes = CreateAesEncryptor(Key);
            using var ms = new MemoryStream();
            byte[] counter = new byte[16];

            using var encryptor = aes.CreateEncryptor();
            byte[] encryptedCounter = ArrayPool<byte>.Shared.Rent(16);

            for (int i = 0; i < plaintext.Length; i += aes.BlockSize / 8)
            {
                encryptor.TransformBlock(counter, 0, counter.Length, encryptedCounter, 0);

                int bytesToEncrypt = Math.Min(plaintext.Length - i, aes.BlockSize / 8);
                byte[] block = new byte[bytesToEncrypt];
                Array.Copy(plaintext, i, block, 0, bytesToEncrypt);

                for (int j = 0; j < bytesToEncrypt; j++)
                    block[j] ^= encryptedCounter[j];

                ms.Write(block, 0, bytesToEncrypt);
                IncrementCounter(counter);
            }

            ArrayPool<byte>.Shared.Return(encryptedCounter);
            return ms.ToArray();
        }

        /// <summary>
        /// Decrypts the provided ciphertext using AES decryption in CTR mode.
        /// </summary>
        /// <param name="cipherText">The data to be decrypted.</param>
        /// <returns>The decrypted plaintext.</returns>
        public byte[] Decrypt(byte[] cipherText)
        {
            using var aes = CreateAesEncryptor(Key);
            using var ms = new MemoryStream(cipherText);
            using var encryptor = aes.CreateEncryptor();

            byte[] counter = new byte[16];
            byte[] encryptedCounter = ArrayPool<byte>.Shared.Rent(16);

            using var resultStream = new MemoryStream();
            byte[] buffer = new byte[16];
            int bytesRead;

            while ((bytesRead = ms.Read(buffer, 0, buffer.Length)) > 0)
            {
                encryptor.TransformBlock(counter, 0, counter.Length, encryptedCounter, 0);

                for (int j = 0; j < bytesRead; j++)
                    buffer[j] ^= encryptedCounter[j];

                resultStream.Write(buffer, 0, bytesRead);
                IncrementCounter(counter);
            }

            ArrayPool<byte>.Shared.Return(encryptedCounter);
            return resultStream.ToArray();
        }

        /// <summary>
        /// Encrypts the provided plaintext asynchronously using AES encryption in CTR mode.
        /// </summary>
        /// <param name="plaintext">The data to be encrypted.</param>
        /// <returns>A task that represents the asynchronous encryption operation, with the encrypted data as the result.</returns>
        public async ValueTask<byte[]> EncryptAsync(byte[] plaintext)
        {
            using var aes = CreateAesEncryptor(Key);
            byte[] iv = new byte[16];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }

            using var ms = new MemoryStream();
            await ms.WriteAsync(iv);

            byte[] counter = new byte[16];
            Array.Copy(iv, counter, iv.Length);
            using var encryptor = aes.CreateEncryptor();

            byte[] encryptedCounter = ArrayPool<byte>.Shared.Rent(16);

            for (int i = 0; i < plaintext.Length; i += aes.BlockSize / 8)
            {
                encryptor.TransformBlock(counter, 0, counter.Length, encryptedCounter, 0);

                int bytesToEncrypt = Math.Min(plaintext.Length - i, aes.BlockSize / 8);
                byte[] block = new byte[bytesToEncrypt];
                Array.Copy(plaintext, i, block, 0, bytesToEncrypt);

                for (int j = 0; j < bytesToEncrypt; j++)
                    block[j] ^= encryptedCounter[j];

                await ms.WriteAsync(block.AsMemory(0, bytesToEncrypt));
                IncrementCounter(counter);
            }

            ArrayPool<byte>.Shared.Return(encryptedCounter);
            return ms.ToArray();
        }

        /// <summary>
        /// Decrypts the provided ciphertext asynchronously using AES decryption in CTR mode.
        /// </summary>
        /// <param name="cipherText">The data to be decrypted.</param>
        /// <returns>A task that represents the asynchronous decryption operation, with the decrypted data as the result.</returns>
        public async ValueTask<byte[]> DecryptAsync(byte[] cipherText)
        {
            using var aes = CreateAesEncryptor(Key);
            byte[] iv = new byte[16];
            Array.Copy(cipherText, 0, iv, 0, iv.Length);

            using var ms = new MemoryStream(cipherText, iv.Length, cipherText.Length - iv.Length);
            using var encryptor = aes.CreateEncryptor();

            byte[] counter = new byte[16];
            Array.Copy(iv, counter, iv.Length);

            using var resultStream = new MemoryStream();
            byte[] buffer = new byte[16];
            int bytesRead;
            byte[] encryptedCounter = ArrayPool<byte>.Shared.Rent(16);

            while ((bytesRead = await ms.ReadAsync(buffer)) > 0)
            {
                encryptor.TransformBlock(counter, 0, counter.Length, encryptedCounter, 0);

                for (int j = 0; j < bytesRead; j++)
                    buffer[j] ^= encryptedCounter[j];

                await resultStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                IncrementCounter(counter);
            }

            ArrayPool<byte>.Shared.Return(encryptedCounter);
            return resultStream.ToArray();
        }

        /// <summary>
        /// Disposes of the resources used by the <see cref="AesCipher"/> instance.
        /// </summary>
        /// <param name="disposing">Indicates whether the method was called directly or from a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    // Release managed resources if necessary
                }
                isDisposed = true;
            }
        }

        /// <summary>
        /// Disposes of the <see cref="AesCipher"/> instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
