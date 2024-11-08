using System.Security.Cryptography;

namespace NETServer.Application.Security;

internal class AesCipher
{
    public byte[] Key { get; private set; }

    public AesCipher(int keySize = 256)
    {
        if (keySize != 128 && keySize != 192 && keySize != 256)
            throw new ArgumentException("Key size must be 128, 192, or 256 bits.");

        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] key = new byte[keySize / 8];
            rng.GetBytes(key);
            this.Key = key;
        }

    }

    /// <summary>
    /// Increments the given counter byte array. This is typically used for 
    /// encryption modes like AES CTR, where a counter is incremented 
    /// for each block of data being encrypted.
    /// </summary>
    /// <param name="counter">The byte array representing the counter to increment.</param>
    private static void IncrementCounter(byte[] counter)
    {
        Span<byte> spanCounter = counter; // Create a span from the counter array for efficient manipulation
                                          // Iterate from the last byte to the first
        for (int i = spanCounter.Length - 1; i >= 0; i--)
        {
            // Increment the current byte
            if (++spanCounter[i] != 0) break; // If the increment did not wrap around (i.e., did not become 0), exit the loop
        }
    }

    /// <summary>
    /// Encrypts data using AES in CTR mode.
    /// </summary>
    /// <param name="plaintext">The plaintext data to encrypt.</param>
    /// <param name="iv">The initialization vector (IV) for encryption.</param>
    /// <returns>The encrypted data as a byte array.</returns>
    public byte[] Encrypt(byte[] plaintext, byte[] iv)
    {
        using Aes aes = Aes.Create();
        aes.Key = this.Key;
        aes.Mode = CipherMode.ECB;

        using var ms = new MemoryStream();
        ms.Write(iv, 0, iv.Length); // Write IV to the start of the encrypted data

        using var encryptor = aes.CreateEncryptor();
        byte[] counter = new byte[16];
        Array.Copy(iv, counter, iv.Length);

        for (int i = 0; i < plaintext.Length; i += aes.BlockSize / 8)
        {
            byte[] encryptedCounter = new byte[16];
            encryptor.TransformBlock(counter, 0, counter.Length, encryptedCounter, 0);

            int bytesToEncrypt = Math.Min(plaintext.Length - i, aes.BlockSize / 8);
            byte[] block = new byte[aes.BlockSize / 8];
            Array.Copy(plaintext, i, block, 0, bytesToEncrypt);

            for (int j = 0; j < bytesToEncrypt; j++)
                block[j] ^= encryptedCounter[j];

            ms.Write(block, 0, bytesToEncrypt);
            IncrementCounter(counter);
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Decrypts data using AES in CTR mode.
    /// </summary>
    /// <param name="cipherText">The encrypted data to decrypt.</param>
    /// <param name="iv">The initialization vector (IV) used during encryption.</param>
    /// <returns>The decrypted data as a byte array.</returns>
    public byte[] Decrypt(byte[] cipherText, byte[] iv)
    {
        using Aes aes = Aes.Create();
        aes.Key = this.Key;
        aes.Mode = CipherMode.ECB;

        using var ms = new MemoryStream(cipherText, iv.Length, cipherText.Length - iv.Length);
        using var encryptor = aes.CreateEncryptor();
        byte[] counter = new byte[16];
        Array.Copy(iv, counter, iv.Length);

        using var resultStream = new MemoryStream();
        byte[] buffer = new byte[16];
        int bytesRead;

        while ((bytesRead = ms.Read(buffer, 0, buffer.Length)) > 0)
        {
            byte[] encryptedCounter = new byte[16];
            encryptor.TransformBlock(counter, 0, counter.Length, encryptedCounter, 0);

            for (int j = 0; j < bytesRead; j++)
                buffer[j] ^= encryptedCounter[j];

            resultStream.Write(buffer, 0, bytesRead);
            IncrementCounter(counter);
        }

        return resultStream.ToArray();
    }

    public async Task<byte[]> EncryptAsync(byte[] plaintext, byte[] iv)
    {
        using Aes aes = Aes.Create();
        aes.Key = this.Key;
        aes.Mode = CipherMode.ECB;

        using var ms = new MemoryStream();
        await ms.WriteAsync(iv, 0, iv.Length);

        using var encryptor = aes.CreateEncryptor();
        byte[] counter = new byte[16];
        Array.Copy(iv, counter, iv.Length);

        for (int i = 0; i < plaintext.Length; i += aes.BlockSize / 8)
        {
            byte[] encryptedCounter = new byte[16];
            encryptor.TransformBlock(counter, 0, counter.Length, encryptedCounter, 0);

            int bytesToEncrypt = Math.Min(plaintext.Length - i, aes.BlockSize / 8);
            byte[] block = new byte[aes.BlockSize / 8];
            Array.Copy(plaintext, i, block, 0, bytesToEncrypt);

            for (int j = 0; j < bytesToEncrypt; j++)
                block[j] ^= encryptedCounter[j];

            await ms.WriteAsync(block, 0, bytesToEncrypt);
            IncrementCounter(counter);
        }

        return ms.ToArray();
    }

    public async Task<byte[]> DecryptAsync(byte[] cipherText, byte[] iv)
    {
        using Aes aes = Aes.Create();
        aes.Key = this.Key;
        aes.Mode = CipherMode.ECB;

        using var ms = new MemoryStream(cipherText, iv.Length, cipherText.Length - iv.Length);
        using var encryptor = aes.CreateEncryptor();
        byte[] counter = new byte[16];
        Array.Copy(iv, counter, iv.Length);

        using var resultStream = new MemoryStream();
        byte[] buffer = new byte[16];
        int bytesRead;

        while ((bytesRead = await ms.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            byte[] encryptedCounter = new byte[16];
            encryptor.TransformBlock(counter, 0, counter.Length, encryptedCounter, 0);

            for (int j = 0; j < bytesRead; j++)
                buffer[j] ^= encryptedCounter[j];

            await resultStream.WriteAsync(buffer, 0, bytesRead);
            IncrementCounter(counter);
        }

        return resultStream.ToArray();
    }
}