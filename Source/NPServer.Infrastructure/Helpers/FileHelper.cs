using NPServer.Infrastructure.Default;
using System.IO;
using System.Text.Json;

namespace NPServer.Infrastructure.Helpers;

/// <summary>
/// Cung cấp các phương thức giúp dễ dàng tải và lưu các tập tin.
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// Trả về đường dẫn tương đối so với thư mục gốc của máy chủ.
    /// </summary>
    /// <param name="filePath">Đường dẫn tập tin đầy đủ.</param>
    /// <returns>Đường dẫn tương đối so với thư mục gốc của máy chủ.</returns>
    public static string GetRelativePath(string filePath)
    {
        return Path.GetRelativePath(PathConfig.Base ?? "", filePath);
    }

    /// <summary>
    /// Deserializes một đối tượng từ tệp JSON tại đường dẫn đã chỉ định.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu cần deserializing.</typeparam>
    /// <param name="path">Đường dẫn đến tệp JSON.</param>
    /// <param name="options">Các tùy chọn JsonSerializer.</param>
    /// <returns>Đối tượng deserialized từ JSON.</returns>
    public static T? DeserializeJson<T>(string path, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<T>(File.ReadAllText(path), options);
    }

    /// <summary>
    /// Serializes một đối tượng <typeparamref name="T"/> thành JSON và lưu vào đường dẫn đã chỉ định.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu cần serializing.</typeparam>
    /// <param name="path">Đường dẫn đến tệp JSON.</param>
    /// <param name="object">Đối tượng cần serializing.</param>
    /// <param name="options">Các tùy chọn JsonSerializer.</param>
    public static void SerializeJson<T>(string path, T @object, JsonSerializerOptions? options = null)
    {
        string? dirName = Path.GetDirectoryName(path);
        if (!Directory.Exists(dirName) && dirName != null)
            Directory.CreateDirectory(dirName);

        string json = JsonSerializer.Serialize(@object, options);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Lưu văn bản vào một tệp văn bản trong thư mục gốc của máy chủ.
    /// </summary>
    /// <param name="fileName">Tên tệp.</param>
    /// <param name="text">Nội dung văn bản cần lưu.</param>
    public static void SaveTextFileToRoot(string fileName, string text)
    {
        File.WriteAllText(Path.Combine(PathConfig.Base, fileName), text);
    }

    /// <summary>
    /// Tạo bản sao lưu cho tệp tin tại đường dẫn chỉ định và giữ số lượng bản sao lưu tối đa.
    /// </summary>
    /// <param name="filePath">Đường dẫn của tệp cần sao lưu.</param>
    /// <param name="maxBackups">Số lượng bản sao lưu tối đa cho phép.</param>
    /// <returns>true nếu tạo bản sao lưu thành công, ngược lại false.</returns>
    public static bool CreateFileBackup(string filePath, int maxBackups)
    {
        if (maxBackups == 0)
            return false;

        if (!File.Exists(filePath))
            return false;

        // Cache backup file names for reuse.
        string[] backupPaths = new string[maxBackups];

        // Look for a free backup index
        int freeIndex = -1;
        for (int i = 0; i < maxBackups; i++)
        {
            backupPaths[i] = $"{filePath}.bak{i}";

            if (!File.Exists(backupPaths[i]))
            {
                freeIndex = i;
                break;
            }
        }

        // Delete the oldest backup if there are no free spots
        if (freeIndex == -1)
        {
            freeIndex = maxBackups - 1;
            File.Delete(backupPaths[freeIndex]);
        }

        // Move files to the right until we free up index 0
        for (int i = freeIndex - 1; i >= 0; i--)
        {
            File.Move(backupPaths[i], backupPaths[i + 1]);
        }

        // Create our backup at index 0
        if (File.Exists(backupPaths[0]))
            return false;

        File.Copy(filePath, backupPaths[0]);

        return true;
    }
}