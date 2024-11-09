using System.Security.Cryptography;

namespace NETServer.Application.Security;

internal class AesCipher
{
    public byte[] Key { get; private set; }

    public AesCipher(int keySize = 256)
    {
        if (keySize != 128 && keySize != 192 && keySize != 256)
            throw new ArgumentException("Key size must be 128, 192, or 256 bits.");

        // Tạo Key ngẫu nhiên
        using (var rng = RandomNumberGenerator.Create())
        {
            this.Key = new byte[keySize / 8];
            rng.GetBytes(this.Key);
        }
    }

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

        // Sử dụng counter bắt đầu từ 0
        byte[] counter = new byte[16];  // Khởi tạo counter với giá trị 0 (hoặc có thể khởi tạo giá trị khác)

        // Tạo encryptor để mã hóa
        using var encryptor = aes.CreateEncryptor();

        for (int i = 0; i < plaintext.Length; i += aes.BlockSize / 8)
        {
            // Mã hóa counter
            byte[] encryptedCounter = new byte[16];
            encryptor.TransformBlock(counter, 0, counter.Length, encryptedCounter, 0);

            // Đoạn dữ liệu cần mã hóa
            int bytesToEncrypt = Math.Min(plaintext.Length - i, aes.BlockSize / 8);
            byte[] block = new byte[bytesToEncrypt];
            Array.Copy(plaintext, i, block, 0, bytesToEncrypt);

            // XOR dữ liệu plaintext với kết quả mã hóa của counter
            for (int j = 0; j < bytesToEncrypt; j++)
                block[j] ^= encryptedCounter[j];

            // Ghi kết quả vào bộ nhớ
            ms.Write(block, 0, bytesToEncrypt);

            // Tăng counter sau mỗi block
            IncrementCounter(counter);
        }

        return ms.ToArray();
    }

    public byte[] Decrypt(byte[] cipherText)
    {
        using var aes = CreateAesEncryptor(this.Key);
        using var ms = new MemoryStream(cipherText);
        using var encryptor = aes.CreateEncryptor();

        // Khởi tạo counter
        byte[] counter = new byte[16];  // Sử dụng counter bắt đầu từ 0 (hoặc có thể khởi tạo giá trị khác)

        using var resultStream = new MemoryStream();
        byte[] buffer = new byte[16];
        int bytesRead;

        while ((bytesRead = ms.Read(buffer, 0, buffer.Length)) > 0)
        {
            // Mã hóa counter
            byte[] encryptedCounter = new byte[16];
            encryptor.TransformBlock(counter, 0, counter.Length, encryptedCounter, 0);

            // XOR dữ liệu cipherText với kết quả mã hóa của counter
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
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(iv); // Tạo IV ngẫu nhiên
        }

        using var ms = new MemoryStream();
        await ms.WriteAsync(iv, 0, iv.Length); // Ghi IV vào đầu

        byte[] counter = new byte[16];
        Array.Copy(iv, counter, iv.Length);
        using var encryptor = aes.CreateEncryptor();

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

    public async Task<byte[]> DecryptAsync(byte[] cipherText)
    {
        using var aes = CreateAesEncryptor(this.Key);
        byte[] iv = new byte[16];
        Array.Copy(cipherText, 0, iv, 0, iv.Length);

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
