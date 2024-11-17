using System.IO.Compression;

namespace NETServer.Network.Packets
{
    public class Gzip
    {
        public static byte[] Compress(byte[] data)
        {
            using var outputStream = new MemoryStream();
            using (var gzip = new GZipStream(outputStream, CompressionLevel.Optimal))
            {
                gzip.Write(data, 0, data.Length);
            }
            return outputStream.ToArray();
        }

        public static byte[] Decompress(byte[] compressedData)
        {
            using var inputStream = new MemoryStream(compressedData);
            using var gzip = new GZipStream(inputStream, CompressionMode.Decompress);
            using var outputStream = new MemoryStream();

            gzip.CopyTo(outputStream);
            return outputStream.ToArray();
        }
    }

    public class Deflate
    {
        // Nén dữ liệu
        public static byte[] Compress(byte[] data)
        {
            using var outputStream = new MemoryStream();
            using (var deflate = new DeflateStream(outputStream, CompressionLevel.Optimal))
            {
                deflate.Write(data, 0, data.Length);
            }
            return outputStream.ToArray();
        }

        // Giải nén dữ liệu
        public static byte[] Decompress(byte[] compressedData)
        {
            using var inputStream = new MemoryStream(compressedData);
            using var deflate = new DeflateStream(inputStream, CompressionMode.Decompress);
            using var outputStream = new MemoryStream();

            deflate.CopyTo(outputStream);
            return outputStream.ToArray();
        }
    }

    public class Brotli
    {
        public static byte[] Compress(byte[] data)
        {
            using var outputStream = new MemoryStream();
            using (var brotli = new BrotliStream(outputStream, CompressionLevel.Optimal))
            {
                brotli.Write(data, 0, data.Length);
            }
            return outputStream.ToArray();
        }

        // Giải nén dữ liệu
        public static byte[] Decompress(byte[] compressedData)
        {
            using var inputStream = new MemoryStream(compressedData);
            using var brotli = new BrotliStream(inputStream, CompressionMode.Decompress);
            using var outputStream = new MemoryStream();

            brotli.CopyTo(outputStream);
            return outputStream.ToArray();
        }
    }
}
