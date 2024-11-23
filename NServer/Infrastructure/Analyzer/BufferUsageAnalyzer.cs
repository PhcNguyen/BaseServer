using System;
using System.IO;

using NServer.Core.Network.Buffers;

namespace NServer.Infrastructure.Analyzer
{
    internal class BufferUsageAnalyzer(MultiSizeBuffer bufferManager)
    {
        private readonly MultiSizeBuffer _bufferManager = bufferManager;

        /// <summary>
        /// Phân tích và ghi thông tin của tất cả các pool buffer vào file CSV.
        /// </summary>
        public void AnalyzeBufferPoolsToCsv(string filePath)
        {
            var bufferSizes = new[] { 256, 512, 1024, 2048, 4096, 8192 };

            using (var writer = new StreamWriter(filePath))
            {
                // Ghi tiêu đề cột vào file CSV
                writer.WriteLine("BufferSize,FreeCount,TotalBuffers,Misses");

                foreach (var size in bufferSizes)
                {
                    try
                    {
                        _bufferManager.GetPoolInfo(size, out var freeCount, out var totalBuffers, out var bufferSize, out var misses);
                        // Ghi dữ liệu buffer vào file CSV
                        writer.WriteLine($"{bufferSize},{freeCount},{totalBuffers},{misses}");
                    }
                    catch (ArgumentException)
                    {
                        // Nếu không có pool cho buffer size, ghi vào CSV là "Not found"
                        writer.WriteLine($"{size},Not found,Not found,Not found");
                    }
                }
            }

            Console.WriteLine("Buffer pool analysis has been written to CSV.");
        }

        /// <summary>
        /// Kiểm tra buffer bằng cách thuê và trả lại một buffer cụ thể.
        /// </summary>
        public void TestBufferUsage(int bufferSize)
        {
            try
            {
                Console.WriteLine($"Testing buffer of size {bufferSize}B...");
                var buffer = _bufferManager.RentBuffer(bufferSize);

                Console.WriteLine($"- Acquired buffer of size: {buffer.Length}B");

                _bufferManager.ReturnBuffer(buffer);
                Console.WriteLine($"- Returned buffer of size: {buffer.Length}B");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}