using NPServer.Core.Interfaces.Memory;
using NPServer.Shared.Services;
using System;

namespace NPServer.Core.Interfaces.Packets;

/// <summary>
/// Lớp cơ sở cho tất cả các gói tin mạng.
/// </summary>
public partial interface IPacket : IPoolable
{
    /// <summary>
    /// Lấy hoặc đặt mã định danh duy nhất của gói tin.
    /// </summary>
    UniqueId Id { get; }

    /// <summary>
    /// Đặt lại gói tin về trạng thái gốc để sẵn sàng tái sử dụng trong pool.
    /// </summary>
    new void ResetForPool();

    /// <summary>
    /// Đặt mã định danh cho gói tin.
    /// </summary>
    /// <param name="id">Mã định danh mới cho gói tin.</param>
    void SetId(UniqueId id);
}
