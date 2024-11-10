using System.Security.Cryptography;

namespace NETServer.Application.Security;

internal class AesCipher
{
    private byte[] Key;

    public AesCipher(int keySize = 256)
    {
        if (keySize != 128 && keySize != 192 && keySize != 256)
            throw new ArgumentException("Key size must be 128, 192, or 256 bits.");

        using (var rng = RandomNumberGenerator.Create())
        {
            this.Key = new byte[keySize / 8];
            rng.GetBytes(this.Key);
        }
    }

    public byte[] GenerateKey(int keySize = 256)
    {
        if (keySize != 128 && keySize != 192 && keySize != 256)
            throw new ArgumentException("Key size must be 128, 192, or 256 bits.");

        // Tạo khóa ngẫu nhiên
        byte[] key = new byte[keySize / 8]; // Kích thước khóa tính theo byte
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }

        return key; // Trả về khóa đã tạo
    }

    public byte[] GetKey() => (byte[])Key.Clone();

    private static void IncrementCounter(byte[] counter)
    {
        // Dùng Span<byte> để thao tác hiệu quả
        for (int i = counter.Length - 1; i >= 0; i--)
        {
            if (++counter[i] != 0) break;
        }
    }

    private static Aes CreateAesEncryptor(byte[] key)
    {
        var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.ECB;
        return aes;
    }

    public byte[] Encrypt(byte[] plaintext)
    {
        using var aes = CreateAesEncryptor(this.Key);
        using var ms = new MemoryStream();

        byte[] counter = new byte[16];  // Khởi tạo counter

        using var encryptor = aes.CreateEncryptor();
        byte[] encryptedCounter = new byte[16]; // Để lưu trữ kết quả mã hóa của counter

        // Sử dụng Span<byte> để tăng hiệu suất
        Span<byte> block = new Span<byte>(new byte[plaintext.Length]);
        plaintext.CopyTo(block);  // Sao chép plaintext vào Span

        for (int i = 0; i < block.Length; i += aes.BlockSize / 8)
        {
            encryptor.TransformBlock(counter, 0, counter.Length, encryptedCounter, 0);

            int bytesToEncrypt = Math.Min(block.Length - i, aes.BlockSize / 8);

            // XOR dữ liệu với kết quả mã hóa của counter
            block.Slice(i, bytesToEncrypt).ForEach((ref byte b, int idx) =>
            {
                b ^= encryptedCounter[idx];
            });

            ms.Write(block.Slice(i, bytesToEncrypt));
            IncrementCounter(counter);
        }

        return ms.ToArray();
    }

    public byte[] Decrypt(byte[] cipherText)
    {
        using var aes = CreateAesEncryptor(this.Key);
        using var ms = new MemoryStream(cipherText);
        using var encryptor = aes.CreateEncryptor();

        byte[] counter = new byte[16]; // Khởi tạo counter
        byte[] encryptedCounter = new byte[16]; // Để lưu trữ kết quả mã hóa của counter

        using var resultStream = new MemoryStream();
        byte[] buffer = new byte[16];
        int bytesRead;

        while ((bytesRead = ms.Read(buffer, 0, buffer.Length)) > 0)
        {
            encryptor.TransformBlock(counter, 0, counter.Length, encryptedCounter, 0);

            // XOR với kết quả mã hóa của counter
            for (int j = 0; j < bytesRead; j++)
                buffer[j] ^= encryptedCounter[j];

            resultStream.Write(buffer, 0, bytesRead);
            IncrementCounter(counter);
        }

        return resultStream.ToArray();
    }

    public async Task<byte[]> EncryptAsync(byte[] plaintext)
    {
        using var aes = CreateAesEncryptor(this.Key);
        byte[] iv = new byte[16];

        // Tạo IV ngẫu nhiên
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(iv);
        }

        using var ms = new MemoryStream();
        await ms.WriteAsync(iv, 0, iv.Length); // Ghi IV vào đầu

        byte[] counter = new byte[16];
        Array.Copy(iv, counter, iv.Length);  // Đưa IV vào counter

        using var encryptor = aes.CreateEncryptor();
        byte[] encryptedCounter = new byte[16];

        Span<byte> block = new Span<byte>(new byte[plaintext.Length]);
        plaintext.CopyTo(block);  // Sao chép plaintext vào Span

        for (int i = 0; i < block.Length; i += aes.BlockSize / 8)
        {
            encryptor.TransformBlock(counter, 0, counter.Length, encryptedCounter, 0);

            int bytesToEncrypt = Math.Min(block.Length - i, aes.BlockSize / 8);

            block.Slice(i, bytesToEncrypt).ForEach((ref byte b, int idx) =>
            {
                b ^= encryptedCounter[idx];
            });

            await ms.WriteAsync(block.Slice(i, bytesToEncrypt)); // Ghi kết quả vào bộ nhớ
            IncrementCounter(counter);
        }

        return ms.ToArray();
    }

    public async Task<byte[]> DecryptAsync(byte[] cipherText)
    {
        using var aes = CreateAesEncryptor(this.Key);
        byte[] iv = new byte[16];
        Array.Copy(cipherText, 0, iv, 0, iv.Length);

        using var ms = new MemoryStream(cipherText, iv.Length, cipherText.Length - iv.Length);
        using var encryptor = aes.CreateEncryptor();

        byte[] counter = new byte[16];
        Array.Copy(iv, counter, iv.Length);  // Đưa IV vào counter

        using var resultStream = new MemoryStream();
        byte[] buffer = new byte[16];
        int bytesRead;

        while ((bytesRead = await ms.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            encryptor.TransformBlock(counter, 0, counter.Length, buffer, 0); // Sử dụng lại bộ đệm

            // XOR với kết quả mã hóa của counter
            for (int j = 0; j < bytesRead; j++)
                buffer[j] ^= buffer[j];  // XOR với chính dữ liệu đã mã hóa

            await resultStream.WriteAsync(buffer, 0, bytesRead);
            IncrementCounter(counter);
        }

        return resultStream.ToArray();
    }
}
