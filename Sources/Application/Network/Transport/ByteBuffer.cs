using System.Buffers;

namespace NETServer.Application.Network.Transport
{
    internal class ByteBuffer
    {
        private readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;
        private int _allocatedBuffers;

        // Thuộc tính trả về số lượng bộ đệm đã cấp phát
        public int Count => _allocatedBuffers;

        // Lấy bộ đệm từ pool
        public byte[] Rent(int requiredSize)
        {
            if (requiredSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(requiredSize), "Kích thước yêu cầu phải lớn hơn 0.");

            // Dùng ArrayPool để thuê bộ đệm, trả về bộ đệm có kích thước phù hợp với yêu cầu
            Interlocked.Increment(ref _allocatedBuffers);
            return _arrayPool.Rent(requiredSize);
        }

        // Trả bộ đệm vào pool
        public void Return(byte[] buffer)
        {
            if (buffer != null)
            {
                // Trả bộ đệm vào ArrayPool để tái sử dụng
                _arrayPool.Return(buffer, clearArray: true); // Xóa dữ liệu trong buffer khi trả lại
                Interlocked.Decrement(ref _allocatedBuffers);
            }
        }
    }
}
