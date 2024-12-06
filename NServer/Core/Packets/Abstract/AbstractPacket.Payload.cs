using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NPServer.Core.Packets.Base;

/// <summary>
/// Lớp cơ sở cho các gói tin với khả năng quản lý dữ liệu payload.
/// </summary>
public partial class AbstractPacket
{
    private Memory<byte> _payload;

    /// <summary>
    /// Dữ liệu chính của gói tin.
    /// </summary>
    public Memory<byte> Payload
    {
        get => _payload;
        protected set
        {
            if (value.Length > int.MaxValue - _headerSize)
                throw new ArgumentOutOfRangeException(nameof(value), "Payload quá lớn.");
            _payload = value;
        }
    }

    /// <summary>
    /// Thử thiết lập dữ liệu payload mới từ một ReadOnlySpan byte.
    /// </summary>
    /// <param name="newPayload">Dữ liệu payload mới.</param>
    /// <returns>True nếu thiết lập thành công, ngược lại False.</returns>
    public bool TrySetPayload(ReadOnlySpan<byte> newPayload)
    {
        if (newPayload.IsEmpty) return false;
        _payload = new Memory<byte>(newPayload.ToArray());
        return true;
    }

    /// <summary>
    /// Thiết lập dữ liệu payload mới từ một chuỗi.
    /// </summary>
    /// <param name="newPayload">Dữ liệu payload mới dưới dạng chuỗi.</param>
    public void SetPayload(string newPayload) =>
        _payload = new Memory<byte>(Encoding.UTF8.GetBytes(newPayload));

    /// <summary>
    /// Thiết lập dữ liệu payload mới từ một Span byte.
    /// </summary>
    /// <param name="newPayload">Dữ liệu payload mới.</param>
    /// <exception cref="ArgumentOutOfRangeException">Ném ra nếu dữ liệu payload quá lớn.</exception>
    public void SetPayload(Span<byte> newPayload)
    {
        if (newPayload.Length > int.MaxValue - _headerSize)
            throw new ArgumentOutOfRangeException(nameof(newPayload), "Payload quá lớn.");
        _payload = new Memory<byte>(newPayload.ToArray());
    }

    /// <summary>
    /// Thêm dữ liệu vào cuối payload hiện tại.
    /// </summary>
    /// <param name="additionalData">Dữ liệu cần thêm.</param>
    public void AppendToPayload(byte[] additionalData)
    {
        var combined = new byte[_payload.Length + additionalData.Length];
        _payload.Span.CopyTo(combined);
        additionalData.CopyTo(combined.AsSpan(_payload.Length));

        _payload = new Memory<byte>(combined);
    }

    /// <summary>
    /// Xóa một phần dữ liệu trong payload.
    /// </summary>
    /// <param name="startIndex">Vị trí bắt đầu của phần cần xóa.</param>
    /// <param name="length">Độ dài của phần cần xóa.</param>
    /// <returns>True nếu xóa thành công, ngược lại False.</returns>
    public bool RemovePayloadSection(int startIndex, int length)
    {
        if (startIndex < 0 || startIndex >= _payload.Length || length < 0 || startIndex + length > _payload.Length)
            return false;

        var newPayload = new byte[_payload.Length - length];
        _payload.Span.Slice(0, startIndex).CopyTo(newPayload);
        _payload.Span.Slice(startIndex + length).CopyTo(newPayload.AsSpan(startIndex));

        _payload = new Memory<byte>(newPayload);
        return true;
    }

    /// <summary>
    /// Thay thế một phần dữ liệu trong payload.
    /// </summary>
    /// <param name="startIndex">Vị trí bắt đầu của phần cần thay thế.</param>
    /// <param name="newData">Dữ liệu mới.</param>
    /// <returns>True nếu thay thế thành công, ngược lại False.</returns>
    public bool ReplacePayloadSection(int startIndex, byte[] newData)
    {
        if (startIndex < 0 || startIndex >= _payload.Length || newData == null)
            return false;

        var newPayload = new byte[_payload.Length - newData.Length + newData.Length];
        _payload.Span.Slice(0, startIndex).CopyTo(newPayload);
        newData.CopyTo(newPayload.AsSpan(startIndex));
        _payload.Span.Slice(startIndex + newData.Length).CopyTo(newPayload.AsSpan(startIndex + newData.Length));

        _payload = new Memory<byte>(newPayload);
        return true;
    }

    /// <summary>
    /// Thêm nhiều payload vào cuối payload hiện tại.
    /// </summary>
    /// <param name="payloads">Danh sách các payload cần thêm.</param>
    public void AppendMultiplePayloads(IEnumerable<byte[]> payloads)
    {
        int totalLength = _payload.Length + payloads.Sum(p => p.Length);
        var combined = new byte[totalLength];

        _payload.Span.CopyTo(combined);
        int offset = _payload.Length;

        foreach (var payload in payloads)
        {
            payload.CopyTo(combined.AsSpan(offset));
            offset += payload.Length;
        }

        _payload = new Memory<byte>(combined);
    }

    /// <summary>
    /// Kiểm tra xem gói tin có hợp lệ hay không.
    /// </summary>
    /// <returns>True nếu gói tin hợp lệ, ngược lại False.</returns>
    public bool IsValid()
    {
        return _payload.Length > _headerSize;
    }
}