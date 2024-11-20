using System.Threading.Channels;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace NETServer.Core.Network.Packet
{
    /// <summary>
    /// Lớp quản lý các channel của người dùng để xử lý gói tin.
    /// </summary>
    internal class PacketContainer : IDisposable
    {
        /// <summary>
        /// Dictionay lưu trữ các channel của người dùng.
        /// </summary>
        public readonly ConcurrentDictionary<Guid, Channel<Packet>> UsersChannels = new();
        private bool _disposed = false;

        /// <summary>
        /// Thêm gói tin vào channel của người dùng.
        /// </summary>
        /// <param name="userId">ID của người dùng.</param>
        /// <param name="packet">Gói tin cần thêm.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPacket(Guid userId, Packet packet)
        {
            var userChannel = UsersChannels.GetOrAdd(userId, _ => Channel.CreateUnbounded<Packet>());
            userChannel.Writer.TryWrite(packet);
        }

        /// <summary>
        /// Xóa tất cả các gói tin có chung id cho một người dùng.
        /// </summary>
        /// <param name="userId">ID của người dùng cần xóa gói tin.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                foreach (var channel in UsersChannels.Values)
                {
                    channel.Writer.Complete();
                }
                UsersChannels.Clear();
            }
        }
    }
}