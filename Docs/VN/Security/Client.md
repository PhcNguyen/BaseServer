# Quy trình gửi và nhận dữ liệu:

## Client:

- Tạo AES key ngẫu nhiên cho phiên giao dịch.
- Mã hóa AES key bằng public key của server.
- Mã hóa dữ liệu bằng AES.
- Gửi public key của client (chỉ gửi một lần).
- Gửi AES key đã mã hóa (chỉ gửi một lần).
- Gửi dữ liệu đã mã hóa bằng AES.


## Server:

- Giải mã AES key từ client bằng private key của server.
- Giải mã dữ liệu bằng AES key đã giải mã.
- Chúng ta sẽ sử dụng RSA để mã hóa AES key và AES để mã hóa dữ liệu.

## Example

### 1. Cấu hình Client 

```csharp
public class Client
{
    private readonly Stream _stream;
    private readonly RSA _rsaClient;
    private readonly Aes _aes;

    public Client(Stream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream), "Stream cannot be null.");
        _rsaClient = RSA.Create(); // Tạo RSA key pair cho client
        _aes = Aes.Create();
        _aes.KeySize = 128; // Hoặc 256 nếu cần
        _aes.GenerateKey();
        _aes.GenerateIV();
    }

    public async Task SendDataAsync(byte[] data)
    {
        try
        {
            // Tạo AES key ngẫu nhiên
            _aes.GenerateKey();
            _aes.GenerateIV();

            // Mã hóa AES key bằng public key của server
            byte[] encryptedAesKey = EncryptWithPublicKey(_rsaClient.ExportRSAPublicKey(), _aes.Key);

            // Mã hóa dữ liệu bằng AES
            byte[] encryptedData = EncryptDataWithAES(data);

            // Gửi public key của client (chỉ gửi một lần)
            await SendPublicKeyAsync();

            // Gửi AES key đã mã hóa và dữ liệu đã mã hóa
            await SendDataAsync(encryptedAesKey, encryptedData);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    private async Task SendPublicKeyAsync()
    {
        byte[] publicKey = _rsaClient.ExportRSAPublicKey();
        await _stream.WriteAsync(publicKey, 0, publicKey.Length);
        await _stream.FlushAsync();
    }

    private async Task SendDataAsync(byte[] encryptedAesKey, byte[] encryptedData)
    {
        // Gửi dữ liệu (AES key đã mã hóa và dữ liệu đã mã hóa)
        var lengthBuffer = BitConverter.GetBytes(encryptedAesKey.Length + encryptedData.Length);
        await _stream.WriteAsync(lengthBuffer, 0, lengthBuffer.Length);

        await _stream.WriteAsync(encryptedAesKey, 0, encryptedAesKey.Length);
        await _stream.WriteAsync(encryptedData, 0, encryptedData.Length);
        await _stream.FlushAsync();
    }

    private byte[] EncryptDataWithAES(byte[] data)
    {
        using (var encryptor = _aes.CreateEncryptor())
        {
            return PerformAESEncryption(data, encryptor);
        }
    }

    private byte[] EncryptWithPublicKey(byte[] publicKey, byte[] aesKey)
    {
        using (var rsa = RSA.Create())
        {
            rsa.ImportRSAPublicKey(publicKey, out _);
            return rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);
        }
    }

    private byte[] PerformAESEncryption(byte[] data, ICryptoTransform encryptor)
    {
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cs.Write(data, 0, data.Length);
        }
        return ms.ToArray();
    }
}
```

### 2. Cấu hình Server
```csharp
public class Server
{
    private readonly Stream _stream;
    private readonly RSA _rsaServer;
    private readonly Aes _aes;

    public Server(Stream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream), "Stream cannot be null.");
        _rsaServer = RSA.Create(); // Tạo RSA key pair cho server
        _aes = Aes.Create();
    }

    public async Task ReceiveDataAsync()
    {
        try
        {
            // Nhận và xử lý public key của client (chỉ cần thực hiện một lần)
            byte[] clientPublicKey = new byte[256]; // 2048-bit RSA key size
            int bytesRead = await _stream.ReadAsync(clientPublicKey, 0, clientPublicKey.Length);

            // Nhận AES key đã mã hóa từ client
            byte[] encryptedAesKey = new byte[256]; // RSA 2048-bit ciphertext size
            await _stream.ReadAsync(encryptedAesKey, 0, encryptedAesKey.Length);

            // Giải mã AES key sử dụng private key của server
            byte[] aesKey = DecryptWithPrivateKey(encryptedAesKey);

            // Nhận dữ liệu đã mã hóa bằng AES
            byte[] encryptedData = new byte[1024];
            bytesRead = await _stream.ReadAsync(encryptedData, 0, encryptedData.Length);

            // Giải mã dữ liệu
            byte[] decryptedData = DecryptDataWithAES(encryptedData, aesKey);

            // Xử lý dữ liệu nhận được
            Console.WriteLine($"Received data: {Encoding.UTF8.GetString(decryptedData)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    private byte[] DecryptWithPrivateKey(byte[] encryptedAesKey)
    {
        return _rsaServer.Decrypt(encryptedAesKey, RSAEncryptionPadding.OaepSHA256);
    }

    private byte[] DecryptDataWithAES(byte[] encryptedData, byte[] aesKey)
    {
        _aes.Key = aesKey;
        using (var decryptor = _aes.CreateDecryptor())
        {
            return PerformAESEncryption(encryptedData, decryptor);
        }
    }

    private byte[] PerformAESEncryption(byte[] data, ICryptoTransform decryptor)
    {
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
        {
            cs.Write(data, 0, data.Length);
        }
        return ms.ToArray();
    }
}
```

## Explain

Client:

- Client tạo một AES key ngẫu nhiên và sử dụng public key của server để mã hóa AES key.
- Dữ liệu được mã hóa với AES key đã tạo và gửi đi.
- Chỉ gửi public key của client một lần, sau đó gửi AES key đã mã hóa và dữ liệu đã mã hóa.

Server:

- Server nhận public key của client và sử dụng private key của server để giải mã AES key.
- Dữ liệu được giải mã bằng AES key vừa giải mã.