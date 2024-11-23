using System;
using System.IO;
using System.Threading.Tasks;

namespace NServer.Infrastructure.Helper
{
    internal static class FileIOHelper
    {
        /// <summary>
        /// Ghi nội dung vào tệp văn bản. 
        /// </summary>
        /// <param name="filePath">Đường dẫn tới tệp.</param>
        /// <param name="content">Nội dung cần ghi.</param>
        /// <param name="append">Thêm vào tệp nếu true, ghi đè nếu false.</param>
        public static void WriteToFile(string filePath, string content, bool append = false)
        {
            try
            {
                if (append)
                {
                    File.AppendAllText(filePath, content);
                }
                else
                {
                    File.WriteAllText(filePath, content);
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Đọc nội dung từ tệp văn bản.
        /// </summary>
        /// <param name="filePath">Đường dẫn tới tệp.</param>
        /// <returns>Nội dung tệp.</returns>
        public static string ReadFromFile(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Ghi dữ liệu byte vào tệp.
        /// </summary>
        /// <param name="filePath">Đường dẫn tới tệp.</param>
        /// <param name="data">Dữ liệu byte cần ghi.</param>
        /// <param name="append">Thêm vào tệp nếu true, ghi đè nếu false.</param>
        public static void WriteBytesToFile(string filePath, byte[] data, bool append = false)
        {
            try
            {
                using var fileStream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write);
                fileStream.Write(data, 0, data.Length);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Đọc dữ liệu byte từ tệp.
        /// </summary>
        /// <param name="filePath">Đường dẫn tới tệp.</param>
        /// <returns>Dữ liệu byte từ tệp.</returns>
        public static byte[] ReadBytesFromFile(string filePath)
        {
            try
            {
                return File.ReadAllBytes(filePath);
            }
            catch (Exception)
            {
                return [];
            }
        }

        /// <summary>
        /// Ghi nội dung vào tệp văn bản bất đồng bộ. 
        /// </summary>
        /// <param name="filePath">Đường dẫn tới tệp.</param>
        /// <param name="content">Nội dung cần ghi.</param>
        /// <param name="append">Thêm vào tệp nếu false, ghi đè nếu true.</param>
        public static async Task WriteToFileAsync(string filePath, string content, bool append = false)
        {
            try
            {
                if (append)
                {
                    await File.AppendAllTextAsync(filePath, content);
                }
                else
                {
                    await File.WriteAllTextAsync(filePath, content);
                }
            }
            catch (Exception ) { }
        }

        /// <summary>
        /// Đọc nội dung từ tệp văn bản bất đồng bộ.
        /// </summary>
        /// <param name="filePath">Đường dẫn tới tệp.</param>
        /// <returns>Nội dung tệp.</returns>
        public static async Task<string> ReadFromFileAsync(string filePath)
        {
            try
            {
                return await File.ReadAllTextAsync(filePath);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Ghi dữ liệu byte vào tệp bất đồng bộ.
        /// </summary>
        /// <param name="filePath">Đường dẫn tới tệp.</param>
        /// <param name="data">Dữ liệu byte cần ghi.</param>
        /// <param name="append">Thêm vào tệp nếu true, ghi đè nếu false.</param>
        public static async Task WriteBytesToFileAsync(string filePath, byte[] data, bool append = false)
        {
            try
            {
                using var fileStream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write);
                await fileStream.WriteAsync(data);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Đọc dữ liệu byte từ tệp bất đồng bộ.
        /// </summary>
        /// <param name="filePath">Đường dẫn tới tệp.</param>
        /// <returns>Dữ liệu byte từ tệp.</returns>
        public static async Task<byte[]> ReadBytesFromFileAsync(string filePath)
        {
            try
            {
                return await File.ReadAllBytesAsync(filePath);
            }
            catch (Exception)
            {
                return [];
            }
        }

        public static bool FileExists(string filePath) => File.Exists(filePath);

        /// <summary>
        /// Kiểm tra sự tồn tại của tệp bất đồng bộ.
        /// </summary>
        /// <param name="filePath">Đường dẫn tới tệp.</param>
        /// <returns>Trả về true nếu tệp tồn tại, ngược lại là false.</returns>
        public static async Task<bool> FileExistsAsync(string filePath)
        {
            return await Task.Run(() => File.Exists(filePath));
        }
    }
}
