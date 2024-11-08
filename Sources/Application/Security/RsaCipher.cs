using System.Security.Cryptography;
using System.Text;
using NETServer.Infrastructure;
using NETServer.Application.Helpers;

namespace NETServer.Application.Security;

/// <summary>
/// Lớp RsaCipher cung cấp các chức năng mã hóa và giải mã sử dụng thuật toán RSA.
/// </summary>
internal class RsaCipher
{
    private static readonly string ExpiryFilePath = Setting.ExpiryDatePath;
    private static readonly TimeSpan KeyRotationInterval = Setting.KeyRotationInterval;
    private readonly string PublicKeyFilePath = Setting.PublicKeyPath;
    private readonly string PrivateKeyFilePath = Setting.PrivateKeyPath;
    private readonly RSA rsa = RSA.Create();

    public RSAParameters PublicKey { get; private set; }
    public RSAParameters PrivateKey { get; private set; }


    /// <summary>
    /// Khởi tạo lớp RsaCipher và tải hoặc tạo khóa RSA mới nếu cần.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (await KeysExist())
        {
            await LoadKeys();
            if (await IsKeyExpired()) await GenerateAndStoreKeys();
        }
        else
        {
            await GenerateAndStoreKeys();
        }
    }

    /// <summary>
    /// Kiểm tra xem các tập tin khóa RSA có tồn tại hay không.
    /// </summary>
    /// <returns>True nếu tất cả các tập tin khóa tồn tại, ngược lại là False.</returns>
    private async Task<bool> KeysExist() =>
        await Task.WhenAll(
            FileHelper.FileExistsAsync(PublicKeyFilePath),
            FileHelper.FileExistsAsync(PrivateKeyFilePath),
            FileHelper.FileExistsAsync(ExpiryFilePath)
        ).ContinueWith(tasks => tasks.Result.All(x => x));

    /// <summary>
    /// Kiểm tra xem khóa RSA đã hết hạn hay chưa.
    /// </summary>
    /// <returns>True nếu khóa đã hết hạn, ngược lại là False.</returns>
    private async Task<bool> IsKeyExpired()
    {
        var expiryDateStr = await FileHelper.ReadFromFileAsync(ExpiryFilePath);
        return DateTime.Now > DateTime.Parse(expiryDateStr);
    }

    /// <summary>
    /// Tạo và lưu trữ cặp khóa RSA mới, đồng thời cập nhật ngày hết hạn.
    /// </summary>
    /// <param name="keySize">Kích thước của khóa RSA (mặc định là 2048 bit).</param>
    private async Task GenerateAndStoreKeys(int keySize = 2048)
    {
        using var rsa = RSA.Create();
        rsa.KeySize = keySize;
        PublicKey = rsa.ExportParameters(false);
        PrivateKey = rsa.ExportParameters(true);

        await SaveKeys();
        await FileHelper.WriteToFileAsync(ExpiryFilePath, DateTime.Now.Add(KeyRotationInterval).ToString());
    }

    /// <summary>
    /// Lưu trữ khóa công khai và bí mật vào các tập tin tương ứng.
    /// </summary>
    private async Task SaveKeys()
    {
        await FileHelper.WriteToFileAsync(PublicKeyFilePath, Convert.ToBase64String(rsa.ExportRSAPublicKey()));
        await FileHelper.WriteToFileAsync(PrivateKeyFilePath, Convert.ToBase64String(rsa.ExportRSAPrivateKey()));
    }

    /// <summary>
    /// Tải khóa công khai và khóa bí mật từ các tập tin tương ứng.
    /// </summary>
    private async Task LoadKeys()
    {
        var publicKeyContent = await FileHelper.ReadFromFileAsync(PublicKeyFilePath);
        var privateKeyContent = await FileHelper.ReadFromFileAsync(PrivateKeyFilePath);

        rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKeyContent), out _);
        rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKeyContent), out _);
        PublicKey = rsa.ExportParameters(false);
        PrivateKey = rsa.ExportParameters(true);
    }

    /// <summary>
    /// Mã hóa văn bản bằng khóa công khai của máy chủ.
    /// </summary>
    /// <param name="plaintext">Chuỗi văn bản cần mã hóa.</param>
    /// <returns>Mảng byte chứa dữ liệu đã được mã hóa.</returns>
    public byte[] Encrypt(string plaintext)
    {
        using var rsaEncryptor = RSA.Create();
        rsaEncryptor.ImportParameters(PublicKey);
        return rsaEncryptor.Encrypt(Encoding.UTF8.GetBytes(plaintext), RSAEncryptionPadding.Pkcs1);
    }

    /// <summary>
    /// Mã hóa văn bản bằng khóa công khai của client.
    /// </summary>
    /// <param name="plaintext">Chuỗi văn bản cần mã hóa.</param>
    /// <param name="publicKeyClient">Khóa công khai của client.</param>
    /// <returns>Mảng byte chứa dữ liệu đã được mã hóa.</returns>
    public static byte[] Encrypt(string plaintext, RSAParameters publicKeyClient)
    {
        using var rsaEncryptor = RSA.Create();
        rsaEncryptor.ImportParameters(publicKeyClient);
        return rsaEncryptor.Encrypt(Encoding.UTF8.GetBytes(plaintext), RSAEncryptionPadding.Pkcs1);
    }

    /// <summary>
    /// Giải mã dữ liệu đã mã hóa bằng khóa bí mật của máy chủ.
    /// </summary>
    /// <param name="encryptedData">Mảng byte chứa dữ liệu đã mã hóa.</param>
    /// <returns>Chuỗi văn bản đã được giải mã.</returns>
    public string Decrypt(byte[] encryptedData) =>
        Encoding.UTF8.GetString(rsa.Decrypt(encryptedData, RSAEncryptionPadding.Pkcs1));
}