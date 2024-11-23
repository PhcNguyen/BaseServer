using System.IO;
using System.IO.Compression;

namespace NServer.Core.Network.Compression
{
    public static class CompressionHelper
    {
        // Nén dữ liệu byte[] thành byte[] nén
        public static byte[]? Compress(byte[] data)
        {
            if (data == null || data.Length == 0)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                // Tạo GZipStream để nén
                using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
                {
                    // Ghi dữ liệu vào GZipStream
                    gzipStream.Write(data, 0, data.Length);
                }

                // Trả về dữ liệu đã nén dưới dạng mảng byte
                return memoryStream.ToArray();
            }
        }

        // Giải nén dữ liệu byte[] nén thành byte[] gốc
        public static byte[]? Decompress(byte[] compressedData)
        {
            if (compressedData == null || compressedData.Length == 0)
                return null;

            using (var memoryStream = new MemoryStream(compressedData))
            using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            using (var outputMemoryStream = new MemoryStream())
            {
                // Giải nén dữ liệu và ghi vào outputMemoryStream
                gzipStream.CopyTo(outputMemoryStream);

                // Trả về mảng byte đã giải nén
                return outputMemoryStream.ToArray();
            }
        }
    }
}