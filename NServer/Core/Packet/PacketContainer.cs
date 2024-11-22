using System.Collections.Concurrent;

namespace NServer.Core.Packet.Utils
{
    /// <summary>
    /// Lớp quản lý các hàng đợi gói tin của người dùng để xử lý gói tin nhanh và hiệu quả.
    /// </summary>
    internal class PacketContainer : IDisposable
    {
        /// <summary>
        /// Dictionary lưu trữ các hàng đợi gói tin của người dùng.
        /// </summary>
        public readonly ConcurrentDictionary<Guid, ConcurrentQueue<Packets>> UsersQueues = new();

        /// <summary>
        /// Thêm gói tin vào hàng đợi của người dùng.
        /// </summary>
        /// <param name="userId">ID của người dùng.</param>
        /// <param name="packet">Gói tin cần thêm.</param>
        public void AddPacket(Guid userId, Packets packet)
        {
            var userQueue = UsersQueues.GetOrAdd(userId, _ => new ConcurrentQueue<Packets>());
            userQueue.Enqueue(packet);
        }

        /// <summary>
        /// Lấy tất cả các gói tin từ hàng đợi của người dùng và xử lý theo nhóm.
        /// </summary>
        /// <param name="userId">ID của người dùng cần lấy gói tin.</param>
        /// <param name="batchSize">Số lượng gói tin cần lấy mỗi lần.</param>
        public List<Packets> GetPacketsBatch(Guid userId, int batchSize)
        {
            if (UsersQueues.TryGetValue(userId, out var queue))
            {
                var batch = new List<Packets>(batchSize);
                while (batch.Count < batchSize && queue.TryDequeue(out var packet))
                {
                    batch.Add(packet);
                }
                return batch;
            }
            return []; // Trả về danh sách rỗng nếu không có gói tin.
        }

        /// <summary>
        /// Giải phóng tài nguyên được sử dụng bởi <see cref="PacketContainer"/>.
        /// </summary>
        public void Dispose()
        {
            UsersQueues.Clear();
            GC.SuppressFinalize(this);
        }
    }
}