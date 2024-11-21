using NServer.Core.Packet;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace NServer.Core.Packet.Utils
{
    /// <summary>
    /// Lớp quản lý các channel của người dùng để xử lý gói tin.
    /// </summary>
    internal class PacketContainer : IDisposable
    {
        /// <summary>
        /// Dictionary lưu trữ các channel của người dùng.
        /// </summary>
        public readonly ConcurrentDictionary<Guid, Channel<Packets>> UsersChannels = new();

        /// <summary>
        /// Thêm gói tin vào channel của người dùng, sắp xếp theo độ ưu tiên.
        /// </summary>
        /// <param name="userId">ID của người dùng.</param>
        /// <param name="packet">Gói tin cần thêm.</param>
        /// <returns>Task đại diện cho hoạt động viết không đồng bộ.</returns>
        public void AddPacket(Guid userId, Packets packet)
        {
            var userChannel = UsersChannels.GetOrAdd(userId, _ => Channel.CreateUnbounded<Packets>());

            try
            {

                Task.Run(async () => await userChannel.Writer.WriteAsync(packet).ConfigureAwait(false));
            }
            catch (ChannelClosedException)
            {
                // Log hoặc xử lý khi channel đã bị đóng.
            }
            catch (Exception ex)
            {
                // Log hoặc xử lý các ngoại lệ khác.
                throw new InvalidOperationException("Failed to write packet to channel.", ex);
            }
        }

        /// <summary>
        /// Xóa tất cả các gói tin có chung id cho một người dùng.
        /// </summary>
        /// <param name="userId">ID của người dùng cần xóa gói tin.</param>
        public void RemovePackets(Guid userId)
        {
            if (UsersChannels.TryRemove(userId, out var channel))
            {
                channel.Writer.Complete();
            }
        }

        /// <summary>
        /// Giải phóng tài nguyên được sử dụng bởi <see cref="PacketContainer"/>.
        /// </summary>
        public void Dispose()
        {
            UsersChannels.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
