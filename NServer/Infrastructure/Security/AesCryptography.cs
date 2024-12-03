using System;
using System.Buffers;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace NServer.Infrastructure.Security;

/// <summary>
/// A class that provides AES encryption and decryption functionality using a key.
/// Implements IDisposable to release resources.
/// </summary>
public static class AesCryptography
{
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
    public static byte[] Encrypt(byte[] key, byte[] plaintext)
    {
        using var aes = CreateAesEncryptor(key);
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
    /// <param name="ciphertext">The data to be decrypted.</param>
    /// <returns>The decrypted plaintext.</returns>
    public static byte[] Decrypt(byte[] key, byte[] ciphertext)
    {
        using var aes = CreateAesEncryptor(key);
        using var ms = new MemoryStream(ciphertext);
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
    public static async ValueTask<byte[]> EncryptAsync(byte[] key, byte[] plaintext)
    {
        using var aes = CreateAesEncryptor(key);
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
    /// <param name="ciphertext">The data to be decrypted.</param>
    /// <returns>A task that represents the asynchronous decryption operation, with the decrypted data as the result.</returns>
    public static async ValueTask<byte[]> DecryptAsync(byte[] key, byte[] ciphertext)
    {
        using var aes = CreateAesEncryptor(key);
        byte[] iv = new byte[16];
        Array.Copy(ciphertext, 0, iv, 0, iv.Length);

        using var ms = new MemoryStream(ciphertext, iv.Length, ciphertext.Length - iv.Length);
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
}