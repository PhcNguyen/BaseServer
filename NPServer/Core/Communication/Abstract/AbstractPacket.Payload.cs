using NPServer.Core.Interfaces.Communication;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NPServer.Core.Communication.Base
{
    /// <summary>
    /// Lớp cơ sở cho các gói tin với khả năng quản lý dữ liệu payload.
    /// </summary>
    public partial class AbstractPacket : IAbstractPacket
    {
        private Memory<byte> _payload;
        private const int MaxDataSize = int.MaxValue - 1024;

        /// <summary>
        /// Dữ liệu chính của gói tin.
        /// </summary>
        public Memory<byte> PayloadData
        {
            get => _payload;
            protected set
            {
                if (value.Length > MaxDataSize)
                    throw new ArgumentOutOfRangeException(nameof(value), "Payload too large.");
                _payload = value;
            }
        }

        /// <summary>
        /// Thiết lập dữ liệu payload mới từ một chuỗi.
        /// </summary>
        /// <param name="newPayload">Dữ liệu payload mới dưới dạng chuỗi.</param>
        public void SetPayload(string newPayload)
        {
            SetPayload(Encoding.UTF8.GetBytes(newPayload));
        }

        /// <summary>
        /// Thiết lập dữ liệu payload mới từ một Span byte.
        /// </summary>
        /// <param name="newPayload">Dữ liệu payload mới.</param>
        public void SetPayload(Span<byte> newPayload)
        {
            if (newPayload.Length > MaxDataSize)
                throw new ArgumentOutOfRangeException(nameof(newPayload), "Payload too large.");
            _payload = new Memory<byte>(newPayload.ToArray());
        }

        /// <summary>
        /// Thêm dữ liệu vào cuối payload hiện tại.
        /// </summary>
        /// <param name="additionalData">Dữ liệu cần thêm.</param>
        public void AddToPayload(ReadOnlyMemory<byte> additionalData)
        {
            if (additionalData.Length + _payload.Length > MaxDataSize)
                throw new ArgumentOutOfRangeException(nameof(additionalData), "Combined payload exceeds size limit.");

            var combinedLength = _payload.Length + additionalData.Length;
            var combined = ArrayPool<byte>.Shared.Rent(combinedLength);

            try
            {
                _payload.Span.CopyTo(combined);
                additionalData.Span.CopyTo(combined.AsSpan(_payload.Length));

                _payload = new Memory<byte>(combined, 0, combinedLength);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(combined);
            }
        }

        /// <summary>
        /// Xóa một phần dữ liệu trong payload.
        /// </summary>
        /// <param name="startIndex">Vị trí bắt đầu của phần cần xóa.</param>
        /// <param name="length">Độ dài của phần cần xóa.</param>
        /// <returns>True nếu xóa thành công, ngược lại False.</returns>
        public bool RemoveFromPayload(int startIndex, int length)
        {
            if (startIndex < 0 || startIndex + length > _payload.Length || length < 0)
                return false;

            var newLength = _payload.Length - length;
            var newPayload = ArrayPool<byte>.Shared.Rent(newLength);
            try
            {
                _payload.Span[..startIndex].CopyTo(newPayload);
                _payload.Span[(startIndex + length)..].CopyTo(newPayload.AsSpan(startIndex));

                _payload = new Memory<byte>(newPayload, 0, newLength);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(newPayload);
            }

            return true;
        }

        /// <summary>
        /// Thay thế một phần dữ liệu trong payload.
        /// </summary>
        /// <param name="startIndex">Vị trí bắt đầu của phần cần thay thế.</param>
        /// <param name="newData">Dữ liệu mới.</param>
        /// <returns>True nếu thay thế thành công, ngược lại False.</returns>
        public bool ReplaceInPayload(int startIndex, ReadOnlyMemory<byte> newData)
        {
            if (startIndex < 0 || startIndex >= _payload.Length || newData.Length == 0)
                return false;

            var newLength = _payload.Length - newData.Length + newData.Length;
            var newPayload = ArrayPool<byte>.Shared.Rent(newLength);
            try
            {
                _payload.Span[..startIndex].CopyTo(newPayload);
                newData.Span.CopyTo(newPayload.AsSpan(startIndex));
                _payload.Span[(startIndex + newData.Length)..].CopyTo(newPayload.AsSpan(startIndex + newData.Length));

                _payload = new Memory<byte>(newPayload, 0, newLength);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(newPayload);
            }

            return true;
        }

        /// <summary>
        /// Thêm nhiều payload vào cuối payload hiện tại.
        /// </summary>
        /// <param name="payloads">Danh sách các payload cần thêm.</param>
        public void AddMultiplePayloads(IEnumerable<ReadOnlyMemory<byte>> payloads)
        {
            int totalLength = _payload.Length + payloads.Sum(p => p.Length);
            if (totalLength > MaxDataSize)
                throw new ArgumentOutOfRangeException(nameof(payloads), "Combined payloads exceed size limit.");

            var combined = ArrayPool<byte>.Shared.Rent(totalLength);
            try
            {
                _payload.Span.CopyTo(combined);
                int offset = _payload.Length;

                foreach (var payload in payloads)
                {
                    payload.Span.CopyTo(combined.AsSpan(offset));
                    offset += payload.Length;
                }

                _payload = new Memory<byte>(combined, 0, totalLength);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(combined);
            }
        }

        /// <summary>
        /// Kiểm tra xem gói tin có hợp lệ hay không.
        /// </summary>
        /// <returns>True nếu gói tin hợp lệ, ngược lại False.</returns>
        public bool IsValid()
        {
            return _payload.Length > 0 && _payload.Length <= MaxDataSize;
        }
    }
}