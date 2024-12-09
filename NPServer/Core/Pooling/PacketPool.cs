using NPServer.Core.Communication;
using NPServer.Core.Interfaces.Communication;
using NPServer.Core.Interfaces.Pooling;
using System;
using System.Threading;
using System.Collections.Concurrent;

namespace NPServer.Core.Pooling
{
    /// <summary>
    /// Quản lý pool của các gói tin để tái sử dụng và tối ưu hóa bộ nhớ một cách an toàn và hiệu quả.
    /// </summary>
    public class PacketPool : IPacketPool, IDisposable
    {
        private readonly ConcurrentBag<IPacket> _pool = [];
        private readonly int _maxCapacity;
        private int _currentCount;
        private readonly Lock _lockObject = new();

        /// <summary>
        /// Khởi tạo một đối tượng <see cref="PacketPool"/> với dung lượng ban đầu và tối đa.
        /// </summary>
        /// <param name="initialCapacity">Số lượng gói tin ban đầu để cấp phát.</param>
        /// <param name="maxCapacity">Số lượng tối đa gói tin trong pool.</param>
        public PacketPool(int initialCapacity, int maxCapacity)
        {
            _maxCapacity = maxCapacity > 0 ? maxCapacity : int.MaxValue - 10000;

            for (int i = 0; i < initialCapacity && i < _maxCapacity; i++)
            {
                _pool.Add(CreatePacket());
                Interlocked.Increment(ref _currentCount);
            }
        }

        /// <summary>
        /// Tạo gói tin mới với các kiểm tra bảo mật.
        /// </summary>
        private static Packet CreatePacket() => new();

        /// <summary>
        /// Lấy gói tin từ pool (nếu có) một cách an toàn.
        /// </summary>
        public IPacket RentPacket()
        {
            if (_pool.TryTake(out var packet))
            {
                Interlocked.Decrement(ref _currentCount);
                return packet;
            }

            // Trường hợp pool hết, tạo mới đối tượng với giới hạn an toàn
            lock (_lockObject)
            {
                if (_currentCount >= _maxCapacity)
                {
                    throw new InvalidOperationException("Đã vượt quá giới hạn pool gói tin.");
                }

                Interlocked.Increment(ref _currentCount);
            }

            return CreatePacket();
        }

        /// <summary>
        /// Trả gói tin vào pool để tái sử dụng với các kiểm tra an toàn.
        /// </summary>
        public void ReturnPacket(IPacket packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet), "Gói tin không được là null.");
            }

            lock (_lockObject)
            {
                if (_currentCount < _maxCapacity)
                {
                    packet.Reset(); // Đảm bảo gói tin được đặt lại trước khi trả về
                    _pool.Add(packet);
                }
                else
                {
                    // Log hoặc xử lý khi pool đã đầy
                    Console.WriteLine("Pool gói tin đã đạt đến giới hạn tối đa.");
                }
            }
        }

        /// <summary>
        /// Kiểm tra số lượng gói tin còn lại trong pool.
        /// </summary>
        public int Count => _currentCount;

        public void Dispose()
        {
            // Thực hiện dọn dẹp tài nguyên nếu cần
            _pool.Clear();
            GC.SuppressFinalize(this);
        }
    }
}