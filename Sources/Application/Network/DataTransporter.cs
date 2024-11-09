using System.Security.Cryptography;
using NETServer.Logging;

namespace NETServer.Application.NetSocketServer;

/*
- 4 bytes đầu tiên là độ dài dữ liệu.
- Public key của server (256 bytes đối với RSA 2048-bit).
- AES key được mã hóa (256 bytes đối với RSA 2048-bit).
- Dữ liệu mã hóa.
- Kích thước tổng cộng sẽ là:
    4 bytes (độ dài) + 256 bytes (public key) + 256 bytes (AES key đã mã hóa) + kích thước dữ liệu mã hóa.
 */
internal class DataTransporter
{
    private readonly Stream _stream;  // Stream dùng để nhận và gửi dữ liệu
    private readonly RSA _serverRsa; // Public key của server (RSA)
    private readonly RSA _clientRsa; // Public key của client (RSA)
    private readonly Aes _aes;       // Khóa AES dùng để mã hóa dữ liệu

    public DataTransporter(Stream stream, RSA serverRsa, RSA clientRsa)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream), "Stream cannot be null.");
        _serverRsa = serverRsa ?? throw new ArgumentNullException(nameof(serverRsa), "Server RSA key cannot be null.");
        _clientRsa = clientRsa ?? throw new ArgumentNullException(nameof(clientRsa), "Client RSA key cannot be null.");
        _aes = Aes.Create() ?? throw new InvalidOperationException("Failed to create AES instance.");
    }

    // Gửi dữ liệu: mã hóa theo yêu cầu
    public async Task SendDataAsync(byte[] data)
    {
        try
        {
            if (data == null || data.Length == 0)
            {
                NLog.Warning("Attempted to send empty data.");
                return;
            }

            if (!_stream.CanWrite)
            {
                NLog.Error("Stream is not writable.");
                return;
            }

            // Mã hóa khóa AES bằng public key của server
            byte[] aesKeyEncrypted = _serverRsa.Encrypt(_aes.Key, RSAEncryptionPadding.OaepSHA256);

            // Tạo dữ liệu đầu ra
            using var ms = new MemoryStream();
            byte[] dataLength = BitConverter.GetBytes(data.Length); // Độ dài dữ liệu 4 bytes
            ms.Write(dataLength, 0, dataLength.Length);  // Ghi độ dài

            // Ghi public key của server (để client có thể mã hóa AES key)
            ms.Write(_serverRsa.ExportRSAPublicKey(), 0, _serverRsa.ExportRSAPublicKey().Length);

            // Ghi mã hóa AES key
            ms.Write(aesKeyEncrypted, 0, aesKeyEncrypted.Length);

            // Mã hóa dữ liệu
            byte[] encryptedData = EncryptData(data);
            ms.Write(encryptedData, 0, encryptedData.Length);

            // Gửi dữ liệu
            await _stream.WriteAsync(ms.ToArray(), 0, (int)ms.Length);
            await _stream.FlushAsync();  // Đảm bảo dữ liệu được gửi ngay lập tức

            NLog.Info($"Sent data: {data.Length} bytes.");
        }
        catch (Exception ex)
        {
            NLog.Error(ex);
            throw;  // Ném lại ngoại lệ nếu cần xử lý ở tầng cao hơn
        }
    }

    // Nhận dữ liệu: giải mã theo yêu cầu
    public async Task<byte[]> ReceiveDataAsync()
    {
        try
        {
            if (!_stream.CanRead)
            {
                NLog.Error("Stream is not readable.");
                return Array.Empty<byte>();
            }

            var buffer = new byte[1024];
            int bytesRead;
            var data = new MemoryStream();

            while ((bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                data.Write(buffer, 0, bytesRead);

                // Nếu đã đọc xong dữ liệu, thoát vòng lặp
                if (data.Length >= 4 && data.Length >= BitConverter.ToInt32(data.ToArray(), 0) + 4)
                {
                    break;
                }
            }

            var allData = data.ToArray();
            if (allData.Length < 4)
            {
                NLog.Error("Received data is too short to contain a valid length field.");
                return Array.Empty<byte>();
            }

            // Đọc độ dài dữ liệu
            int dataLength = BitConverter.ToInt32(allData, 0);
            if (allData.Length < dataLength + 4)
            {
                NLog.Error("Received data does not contain the full expected data.");
                return Array.Empty<byte>();
            }

            // Đọc public key của client
            int publicKeyLength = _clientRsa.ExportRSAPublicKey().Length;
            byte[] clientPublicKey = new byte[publicKeyLength];
            Array.Copy(allData, 4, clientPublicKey, 0, publicKeyLength);
            _clientRsa.ImportRSAPublicKey(clientPublicKey, out _);

            // Đọc mã hóa AES key
            int aesKeyLength = _clientRsa.Encrypt(_aes.Key, RSAEncryptionPadding.OaepSHA256).Length;
            byte[] aesKeyEncrypted = new byte[aesKeyLength];
            Array.Copy(allData, 4 + publicKeyLength, aesKeyEncrypted, 0, aesKeyLength);
            byte[] aesKey = _clientRsa.Decrypt(aesKeyEncrypted, RSAEncryptionPadding.OaepSHA256);

            // Thiết lập lại AES key đã giải mã
            _aes.Key = aesKey;

            // Đọc dữ liệu đã mã hóa
            byte[] encryptedData = new byte[dataLength];
            Array.Copy(allData, 4 + publicKeyLength + aesKeyLength, encryptedData, 0, dataLength);

            return DecryptData(encryptedData);
        }
        catch (Exception ex)
        {
            NLog.Error(ex);
            throw;  // Ném lại ngoại lệ nếu cần xử lý ở tầng cao hơn
        }
    }

    // Mã hóa dữ liệu với AES
    private byte[] EncryptData(byte[] data)
    {
        using var encryptor = _aes.CreateEncryptor();
        return PerformCryptography(data, encryptor);
    }

    // Giải mã dữ liệu với AES
    private byte[] DecryptData(byte[] data)
    {
        using var decryptor = _aes.CreateDecryptor();
        return PerformCryptography(data, decryptor);
    }

    // Thực hiện mã hóa hoặc giải mã
    private byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
    {
        using var ms = new MemoryStream();
        using var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write);
        cryptoStream.Write(data, 0, data.Length);
        cryptoStream.FlushFinalBlock();
        return ms.ToArray();
    }
}
