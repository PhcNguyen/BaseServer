using System;
using System.Collections.Generic;

namespace NPServer.Core.Interfaces.Packets;

public partial interface IPacket
{
    /// <summary>
    /// Lấy dữ liệu của payload dưới dạng bộ nhớ byte.
    /// </summary>
    Memory<byte> PayloadData { get; }

    /// <summary>
    /// Đặt payload từ một chuỗi mới.
    /// </summary>
    /// <param name="newPayload">Chuỗi dữ liệu mới cho payload.</param>
    void SetPayload(string newPayload);

    /// <summary>
    /// Đặt payload từ một span byte mới.
    /// </summary>
    /// <param name="newPayload">Span dữ liệu mới cho payload.</param>
    void SetPayload(Span<byte> newPayload);

    /// <summary>
    /// Thêm dữ liệu bổ sung vào payload.
    /// </summary>
    /// <param name="additionalData">Dữ liệu bổ sung cần thêm vào payload.</param>
    void AddToPayload(ReadOnlyMemory<byte> additionalData);

    /// <summary>
    /// Xóa dữ liệu từ payload bắt đầu từ chỉ số và chiều dài xác định.
    /// </summary>
    /// <param name="startIndex">Chỉ số bắt đầu xóa.</param>
    /// <param name="length">Chiều dài dữ liệu cần xóa.</param>
    /// <returns>Trả về `true` nếu xóa thành công, ngược lại `false`.</returns>
    bool RemoveFromPayload(int startIndex, int length);

    /// <summary>
    /// Thay thế dữ liệu trong payload từ một vị trí xác định bằng dữ liệu mới.
    /// </summary>
    /// <param name="startIndex">Chỉ số bắt đầu thay thế.</param>
    /// <param name="newData">Dữ liệu mới thay thế vào payload.</param>
    /// <returns>Trả về `true` nếu thay thế thành công, ngược lại `false`.</returns>
    bool ReplaceInPayload(int startIndex, ReadOnlyMemory<byte> newData);

    /// <summary>
    /// Thêm nhiều payload vào danh sách payload.
    /// </summary>
    /// <param name="payloads">Danh sách dữ liệu payload cần thêm vào.</param>
    void AddMultiplePayloads(IEnumerable<ReadOnlyMemory<byte>> payloads);
}
